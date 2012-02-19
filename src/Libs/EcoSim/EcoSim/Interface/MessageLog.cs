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
using Microsoft.Xna.Framework.Graphics;

namespace Sim.Interface
{
    public class MessageLog
    {
        private List<Message> messages;                     // list of error messages to be printed
        private Boolean messagesPresent;                    // if the error list is not empty
        private float messageMinDuration = 10.0f;           // shortest duration a message can appear for

        private Color errorMessageColor = new Color(255, 200, 200); // color of error messages
        private Color infoMessageColor = new Color(200, 255, 200);  // color of informational messages

        public MessageLog()
        {
            messages = new List<Message>();
        }

        /// <summary>
        /// Adds an error message to the message list
        /// </summary>
        public void AddErrorMessage(object src, String text)
        {
            AddMessage(src, text, errorMessageColor);
        }

        /// <summary>
        /// Adds an information message to the message list
        /// </summary>
        public void AddInfoMessage(object src, String text)
        {
            AddMessage(src, text, infoMessageColor);
        }

        public void AddMessage(object src, String text, Color color)
        {
            String msg = String.Format("({0}): {1}", src.GetType().Name, text);
            messages.Insert(0, new Message(msg, messageMinDuration, 0.75f, color));
            messagesPresent = true;
        }

        public void Draw(SpriteBatch sb)
        {
            if (messagesPresent)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    Message m = messages[i];
                    Color color = m.Color;
                    sb.DrawString(Fonts.Arial, m.MessageString, new Vector2(1, i * 20), new Color(0, 0, 0, color.A));
                    sb.DrawString(Fonts.Arial, m.MessageString, new Vector2(0, i * 20), color);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (messagesPresent)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    Message error = (Message)messages[i];
                    if (error.Expired)
                    {
                        messages.Remove(error);
                        i--;
                    }
                    else
                        error.Update(gameTime);
                }
            }
        }
    }
}
