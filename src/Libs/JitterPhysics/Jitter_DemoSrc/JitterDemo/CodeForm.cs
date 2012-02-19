#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using Jitter.Dynamics;
using Jitter;
#endregion

namespace JitterDemo
{
    public partial class CodeForm : Form
    {
        private World world;

        public CodeForm(World world)
        {
            this.world = world;
            InitializeComponent();
        }

        public string CompleteCode()
        {
            #region Build code string
            string code =
            @"using System;" + "\r\n" +
            @"using System.Collections.Generic;" + "\r\n" +
            @"using System.Text;" + "\r\n" +
            @"using Jitter;" + "\r\n" +
            @"using Microsoft.Xna.Framework;" + "\r\n" +
            @"using Jitter.LinearMath;" + "\r\n" +
            @"using Jitter.Dynamics;" + "\r\n" +
            @"using Jitter.Collision.Shapes;" + "\r\n" +
            @"using Jitter.Dynamics.Constraints;" + "\r\n" +
            @"using Microsoft.Xna.Framework.Graphics;" + "\r\n" +
            @"using Jitter.Collision;" + "\r\n" +
            @"namespace JitterDemo" + "\r\n" +
            @"{" + "\r\n" +
            @"    class UserCode" + "\r\n" +
            @"    {" + "\r\n" +
            textBox1.Text + "\r\n" +
            @"        }" + "\r\n" +
            @"    }";
            #endregion

            return code;
        }

        #region private void TryToRunUserTestCode(string code)
        public void TryToRunUserTestCode(string code)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters cp = new CompilerParameters();

            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string location = assembly.Location;
                    if (!String.IsNullOrEmpty(location))
                    {
                        compilerparams.ReferencedAssemblies.Add(location);
                    }
                }
                catch (NotSupportedException)
                {
                    // this happens for dynamic assemblies, so just ignore it.
                }
            }

            CompilerResults results =
               provider.CompileAssemblyFromSource(compilerparams, code);

            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n",
                           error.Line - 15, error.Column, error.ErrorText);
                }

                int firstErrorLine = results.Errors[0].Line - 16;

                if (firstErrorLine >= 0)
                {
                    int firstChar = textBox1.GetFirstCharIndexFromLine(firstErrorLine);
                    int selLength = textBox1.Lines[firstErrorLine].Length;
                    textBox1.Select(firstChar, selLength);
                }

                System.Windows.Forms.MessageBox.Show(errors.ToString(), "Source failed to compile", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
            }
            else
            {
                this.Close();

                #region Remove everything from the world
                List<RigidBody> toBeRemoved = new List<RigidBody>();
                foreach (RigidBody body in world.RigidBodies)
                {
                    if (body.Tag is Boolean) continue;
                    toBeRemoved.Add(body);
                }

                foreach (RigidBody body in toBeRemoved)
                {
                    world.RemoveBody(body);
                }
                #endregion

                try
                {
                    Object o = results.CompiledAssembly.CreateInstance("JitterDemo.UserCode", false,
                        BindingFlags.ExactBinding,
                        null, new Object[] { }, null, null);

                    MethodInfo m = results.CompiledAssembly.GetType("JitterDemo.UserCode").GetMethod("Create");
                    Object ret = m.Invoke(o, new Object[] { world });
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Your code did compile but causes " +
                        "an exception: \r\n\r\n" + ex.ToString(),"Exception caused by user code",MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }

        }
        #endregion

        #region Load and Save

        private void loadSceneCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Jitter Scene (*.jscene)|*.jscene";
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    textBox1.Text = File.ReadAllText(openFileDialog1.FileName);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Something went wrong: " + ex.ToString());
                }
            }
        }

        private void saveSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Jitter Scene (*.jscene)|*.jscene";

            DialogResult result = saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog1.FileName, textBox1.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Something went wrong: " + ex.ToString());
                }
            }
        }
        #endregion

        private void compileAndRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TryToRunUserTestCode(CompleteCode());
        }


    }
}
