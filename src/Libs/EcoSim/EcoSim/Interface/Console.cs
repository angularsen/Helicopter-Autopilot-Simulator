/* 
 * Environment Simulator
 * Copyright (C) 2008-2009 Justin Stoecker
 * 
 * This program is free software; you can redistribute it and/or modify it under the terms of the 
 * GNU General Public License as published by the Free Software Foundation; either version 2 of 
 * the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace Sim.Interface
{
    public class Console
    {
        private SpriteFont font = Fonts.Tahoma;
        private SimEngine sim;
        private Rectangle area;
        private Rectangle cursor;

        private String currentCommand = String.Empty;
        private List<String> commandHistory = new List<string>();
        private int historyIndex = 0;
        private int maxHistorySize = 30;

        private bool open;

        public bool Open { get { return open; } }

        public enum Commands
        {
            INVOKE,
            FPS,
            QUIT,
        };

        public Console(SimEngine sim)
        {
            this.sim = sim;
            area = new Rectangle(0, 0, sim.GraphicsDevice.Viewport.Width, (int)font.MeasureString("|").Y);
            cursor = new Rectangle(0, area.Bottom - 2, (int)font.MeasureString("  ").X, 2);
        }

        public void Update()
        {
            UIKeyboard keyboard = sim.UI.Keyboard;
            Keys[] pressedKeys = keyboard.State.GetPressedKeys();
            for (int i = 0; i < pressedKeys.Length; i++)
            {
                Keys k = pressedKeys[i];

                // check pushed keys
                if (keyboard.KeyPushed(k)) {
                    if (k == Keys.Enter)
                    {
                        if (open && currentCommand != String.Empty)
                            ExecuteCommand();
                        else
                            open = !open;
                    }
                    else if (k == Keys.Delete)
                        ClearCommand();
                }

                // check typed keys for input
                if (open && keyboard.KeyTyped(k))
                {
                    if (k == Keys.Back)
                        RemoveCommandChar();
                    else if (k == Keys.Up)
                        SelectPrevHistoryCmd();
                    else if (k == Keys.Down)
                        SelectNextHistoryCmd();
                    else
                    {
                        char? c = keyboard.KeyToChar(k);
                        if (c.HasValue)
                            AddCommandChar(c.Value);
                    }
                }
            }
        }

        private void SelectPrevHistoryCmd()
        {
            if (commandHistory.Count > 0)
            {
                historyIndex = ++historyIndex < commandHistory.Count ? historyIndex : commandHistory.Count - 1;
                currentCommand = commandHistory[historyIndex];
                AdjustCursor();
            }
        }

        private void SelectNextHistoryCmd()
        {
            if (commandHistory.Count > 0)
            {
                historyIndex = --historyIndex > -1 ? historyIndex : -1;
                currentCommand = historyIndex > -1 ? commandHistory[historyIndex] : String.Empty;
                AdjustCursor();
            }
        }

        private void AddCommandToHistory()
        {
            if (maxHistorySize > 0)
            {
                commandHistory.Insert(0, currentCommand);
                historyIndex = -1;
                if (commandHistory.Count > maxHistorySize)
                    commandHistory.RemoveAt(maxHistorySize);
            }
        }

        /// <summary>
        /// Executes the current command string
        /// </summary>
        public void ExecuteCommand()
        {
            String[] parameters = currentCommand.Split(new char[] { ' ' });
            String cmdName = parameters[0];
            
            try
            {
                switch ((Commands)Enum.Parse(typeof(Commands), cmdName.ToUpper()))
                {
                    case Commands.INVOKE:
                        Invoke(parameters);
                        break;
                    case Commands.FPS:
                        sim.UI.ToggleFPS();
                        break;
                    case Commands.QUIT:
                        sim.Exit();
                        break;
                };
            }
            catch
            {
                SendError("Unrecognized command: " + currentCommand);
            }

            AddCommandToHistory();
            ClearCommand();
        }

        private void AddCommandChar(char c)
        {
            currentCommand += c;
            AdjustCursor();
        }

        private void RemoveCommandChar()
        {
            if (currentCommand.Length > 0)
            {
                currentCommand = currentCommand.Remove(currentCommand.Length - 1);
                AdjustCursor();
            }
        }

        private void ClearCommand()
        {
            currentCommand = String.Empty;
            AdjustCursor();
        }

        private void AdjustCursor()
        {
            cursor.X = (int)Fonts.Tahoma.MeasureString(currentCommand).X;
        }

        private void Invoke(object[] parameters)
        {
            String className, methodName;
            Type[] methodParamTypes = new Type[0];
            object[] methodParams = null;
            object classInstance;
            MethodBase method;
           
            try
            {
                className = parameters[1].ToString();   // get the name of the class
                methodName = parameters[2].ToString();  // get the name of the method
            }
            catch (IndexOutOfRangeException)
            { SendError("use format \"invoke <class> <method> <param1> <param2> ... <paramN>\""); return; }

            if (parameters.Length > 3)
            {
                methodParams = new object[parameters.Length - 3];
                methodParamTypes = new Type[methodParams.Length];
            }

            // if parameters exist, get their values and types
            if (methodParams != null)
            {
                for (int i = 0; i < methodParams.Length; i++)
                {
                    methodParams[i] = ParseParameter(parameters[i + 3].ToString());
                    methodParamTypes[i] = methodParams[i].GetType();
                }
            }

            // retrieve the desired class
            try { classInstance = GetClassInstance(className, sim); }
            catch (Exception) { SendError("class not found: " + className); return; }

            // if the method is a property, display its value
            try
            {
                PropertyInfo p = classInstance.GetType().GetProperty(methodName);
                MethodBase b = p.GetGetMethod();
                try { SendInfo(b.Invoke(classInstance, methodParams).ToString()); }
                catch (Exception) { SendInfo("null"); }
                return;
            }
            catch (Exception) { }

            // otherwise, retrieve the method
            method = classInstance.GetType().GetMethod(methodName, methodParamTypes);
            if (method == null)
            {
                String error = "method not found: " + className + "." + methodName + "(";
                if (methodParams != null)
                    error += CommaSeparatedList(methodParamTypes);
                SendError(error + ")");
                return;
            }

            // and invoke the method
            if (method != null)
            {
                if (classInstance.GetType().GetMethod(methodName, methodParamTypes).ReturnType == typeof(void))
                {
                    method.Invoke(classInstance, methodParams);
                    String info = "method invoked: " + className + "." + methodName + "(";
                    if (methodParams != null)
                        info += CommaSeparatedList(methodParams);
                    SendInfo(info + ")");
                }
                else
                {
                    try { SendInfo(method.Invoke(classInstance, methodParams).ToString()); }
                    catch (Exception) { SendInfo("null"); }
                }
            }
        }

        private String CommaSeparatedList(object[] objects)
        {
            String list = "";

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects.GetType() == typeof(Type[]))
                    list += ((Type)objects[i]).Name;
                else
                    list += objects[i];
                if (i != objects.Length - 1)
                    list += ',';
            }
            return list;
        }

        private object GetClassInstance(String classPath, object root)
        {
            object currentClassInstance;
            if (classPath.Contains("."))
            {
                String currentClass = classPath.Remove(classPath.IndexOf('.'));
                String restOfClassPath = classPath.Substring(classPath.IndexOf('.') + 1);
                currentClassInstance = root.GetType().GetProperty(currentClass).GetGetMethod().Invoke(root, null);
                return GetClassInstance(restOfClassPath, currentClassInstance);
            }
            return root.GetType().GetProperty(classPath).GetGetMethod().Invoke(root, null);
        }

        private Object ParseParameter(String parameter)
        {
            if (parameter.Equals(""))
                return null;

            // if the parameter is in the form [*] it is some kind of data structure
            if (parameter[0] == '[' && parameter[parameter.Length - 1] == ']')
            {
                try
                {
                    String[] pValues = parameter.Split(new char[] { ',' });
                    pValues[0] = pValues[0].Substring(1);
                    pValues[pValues.Length - 1] = pValues[pValues.Length - 1].Remove(pValues[pValues.Length - 1].Length - 1);
                    float[] digits = new float[pValues.Length];

                    // parse each float
                    for (int i = 0; i < pValues.Length; i++)
                        digits[i] = float.Parse(pValues[i]);

                    // if the parameter is in the form [#,#] it is a vector
                    if (pValues.Length == 2)
                        return new Vector2(digits[0], digits[1]);

                    if (pValues.Length == 3)
                        return new Vector3(digits[0], digits[1], digits[2]);

                    // if the parameter is in the form [#,#,#,#] it is a rectanglef
                    if (pValues.Length == 4)
                        return new Rectangle((int)digits[0], (int)digits[1], (int)digits[2], (int)digits[3]);
                }
                catch (Exception) { SendError("use [#,#] for vector2s and [#,#,#,#] for rectangles"); }
            }

            // check if parameter can be parsed as a number
            if (parameter.Contains("."))
            {
                try { return float.Parse(parameter); }
                catch (FormatException) { }
            }
            else
            {
                try { return int.Parse(parameter); }
                catch (FormatException) { }
            }

            // check if parameter is a boolean
            if (parameter.ToLower().Equals("false"))
                return false;
            if (parameter.ToLower().Equals("true"))
                return true;

            // otherwise, leave it as a string
            return parameter;
        }

        /// <summary>
        /// Custom error message to message log
        /// </summary>
        public void SendError(String s)
        {
            sim.UI.MessageLog.AddErrorMessage(this,s);
        }

        /// <summary>
        /// Sends information message to message log
        /// </summary>
        public void SendInfo(String s)
        {
            sim.UI.MessageLog.AddInfoMessage(this,s);
        }

        public void Draw(GameTime gameTime, SpriteBatch sb)
        {
            if (open)
            {
                sb.Draw(UserInterface.TexBlank, area, new Color(0, 0, 0, 175));
                sb.Draw(UserInterface.TexBlank, cursor, Color.LightBlue);
                sb.DrawString(font, currentCommand, new Vector2(area.X, area.Y), Sim.Settings.Colors.Default.Text);
            }
        }
    }
}
