using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace VTFucker
{
    public partial class ShaderEditorForm : Form
    {
        Renderer renderer;
        public ShaderEditorForm(string code, Renderer r)
        {
            InitializeComponent();
            renderer = r;
            ShaderEditor.Text = code;
        }
        public void SetShaderCode(string code)
        {
            ShaderEditor.Text = code;

        }
        private void CompileButton_Click(object sender, EventArgs e)
        {
            //renderer.CreateShaders(ShaderEditor.Text);
           // renderer.UpdateRenderer = true;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            File.WriteAllText("vteffect.fx", ShaderEditor.Text);
            //renderer.CreateShaders(ShaderEditor.Text);
            //renderer.UpdateRenderer = true;
        }
    }
}
