﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BFtools
{
    public partial class Form1 : Form
    {
		private readonly string userProfilePath = Environment.GetEnvironmentVariable("userprofile") + @"\Documents\Battlefield 3\settings\PROF_SAVE_profile";
		private readonly string screenshotsPath = Environment.GetEnvironmentVariable("userprofile") + @"\Documents\Battlefield 3\screenshots";
		private readonly string tempSettings = Environment.GetEnvironmentVariable("temp") + @"\bf3.ini";
		private readonly string dxdiagOutput = Environment.GetEnvironmentVariable("temp") + @"\dxdiag.tmp";

		private readonly RegistryKey key;

		private string BFdirectory = "";

        public Form1()
        {
            InitializeComponent();

			key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\EA Games\Battlefield 3", true);  // 64bit
			if (key == null || key.GetValue("Install Dir") == null)
				key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\EA GAMES\Battlefield 3", true);  // 32bit
			if (key == null || key.GetValue("Install Dir") == null)
				key = null;  // not found
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MaximumSize = Size;
            comboBox2.SelectedIndex = 1;
            comboBox4.SelectedIndex = 0;

            if (File.Exists(userProfilePath))
            {
                string[] lines = File.ReadAllLines(userProfilePath);
                foreach (string line in lines)
                {
                    if (line.Contains("GstRender.VSyncEnabled 0"))
						checkBox7.CheckState = CheckState.Unchecked;

                    if (line.Contains("GstRender.VSyncEnabled 1"))
						checkBox7.CheckState = CheckState.Checked;
                }
            }

	        if (File.Exists(tempSettings))
            {
                string[] lines = File.ReadAllLines(tempSettings);
                BFdirectory = lines[0];
                if (File.Exists(BFdirectory + @"\user.cfg"))
					ReadCFG();
            }
            else if (key != null)
            {
                BFdirectory = (string)key.GetValue("Install Dir");
                if (File.Exists(BFdirectory + @"\user.cfg"))
					ReadCFG();

                switch (key.GetValue("Locale").ToString())
                {
                    case "cs_CZ": comboBox1.SelectedIndex = 0; break;
                    case "de_DE": comboBox1.SelectedIndex = 1; break;
                    case "en_US": comboBox1.SelectedIndex = 2; break;
                    case "es_Es": comboBox1.SelectedIndex = 3; break;
                    case "fr_FR": comboBox1.SelectedIndex = 4; break;
                    case "it_IT": comboBox1.SelectedIndex = 5; break;
                    case "pl_PL": comboBox1.SelectedIndex = 6; break;
                    case "ko_KO": comboBox1.SelectedIndex = 7; break;
                    case "ja_JA": comboBox1.SelectedIndex = 8; break;
                    case "zh_ZH": comboBox1.SelectedIndex = 9; break;
                }
            }
            else if (LoadPath())
            {
				ReadCFG();
            }

	        if (comboBox3.SelectedIndex == -1)
	        {
				try
				{
					int DXversion = CheckDxVersion();

					switch (DXversion)
					{
						case 10:
							comboBox3.SelectedIndex = 0;
							break;
						case 11:
							comboBox3.SelectedIndex = 1;
							break;
					}
				}
				catch {}
	        }
        }

        private bool LoadPath()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                BFdirectory = folderBrowserDialog1.SelectedPath;
				using (StreamWriter file = new StreamWriter(tempSettings, false))
                    file.Write(BFdirectory);
                return true;
            }
            else 
				return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
			if (BFdirectory.Length == 0 && !LoadPath())
				return;

            using (StreamWriter file = new StreamWriter(BFdirectory + @"\user.cfg", false))
            {
                file.WriteLine("GameTime.MaxVariableFps " + numericUpDown1.Value);
				file.WriteLine("UI.DrawEnable " + BoolToInt(checkBox4.Checked));
				file.WriteLine("Render.DrawFPS " + BoolToInt(checkBox1.Checked));

	            if (checkBox8.Checked)
                {
                    file.WriteLine("Render.PerfOverlayEnable 1");
                    file.WriteLine("Render.PerfOverlayVisible 1");
                }
                else
                {
                    file.WriteLine("Render.PerfOverlayEnable 0");
                    file.WriteLine("Render.PerfOverlayVisible 0");
                }

                switch (checkBox7.CheckState)
                {
	                case CheckState.Checked:
		                file.WriteLine("RenderDevice.TripleBufferingEnable 1");
		                break;
	                case CheckState.Unchecked:
		                file.WriteLine("RenderDevice.TripleBufferingEnable 0");
		                break;
                }

                switch (comboBox3.SelectedIndex)
                {
	                case 0:
		                file.WriteLine("RenderDevice.Dx11Enable 0");
		                break;
	                case 1:
		                file.WriteLine("RenderDevice.Dx11Enable 1");
		                break;
                }

				file.WriteLine("RenderDevice.ForceRenderAheadLimit " + (comboBox4.SelectedIndex - 1));
				file.WriteLine("WorldRender.FxaaEnable " + BoolToInt(checkBox3.Checked));
				file.WriteLine("WorldRender.DxDeferredCsPathEnable " + BoolToInt(checkBox5.Checked));
				file.WriteLine("WorldRender.TransparencyShadowmapsEnable " + BoolToInt(checkBox9.Checked));
				file.WriteLine("WorldRender.SpotlightShadowmapEnable " + BoolToInt(checkBox2.Checked));

	            switch (comboBox2.SelectedIndex)
                {
                    case 0: file.WriteLine("WorldRender.SpotLightShadowmapResolution 64"); break;
                    case 1: file.WriteLine("WorldRender.SpotLightShadowmapResolution 256"); break;
                    case 2: file.WriteLine("WorldRender.SpotLightShadowmapResolution 1024"); break;
                    case 3: file.WriteLine("WorldRender.SpotLightShadowmapResolution 2048"); break;
                    case 4: file.WriteLine("WorldRender.SpotLightShadowmapResolution 4096"); break;
                    case 5: file.WriteLine("WorldRender.SpotLightShadowmapResolution 8192"); break;
                }
            }

            if (key != null)
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0: key.SetValue("Locale", "cs_CZ", RegistryValueKind.String); break;
                    case 1: key.SetValue("Locale", "de_DE", RegistryValueKind.String); break;
                    case 2: key.SetValue("Locale", "en_US", RegistryValueKind.String); break;
                    case 3: key.SetValue("Locale", "es_ES", RegistryValueKind.String); break;
                    case 4: key.SetValue("Locale", "fr_FR", RegistryValueKind.String); break;
                    case 5: key.SetValue("Locale", "it_IT", RegistryValueKind.String); break;
                    case 6: key.SetValue("Locale", "pl_PL", RegistryValueKind.String); break;
                    case 7: key.SetValue("Locale", "ko_KO", RegistryValueKind.String); break;
                    case 8: key.SetValue("Locale", "ja_JA", RegistryValueKind.String); break;
                    case 9: key.SetValue("Locale", "zh_ZH", RegistryValueKind.String); break;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (File.Exists(BFdirectory + @"\user.cfg"))
				File.Delete(BFdirectory + @"\user.cfg");

			if (File.Exists(tempSettings))
				File.Delete(tempSettings);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (BFdirectory.Length == 0)
            {
                if (LoadPath())
					ReadCFG();
                else
					return;
            }

            try
            {
	            Process BFstart = new Process
	            {
		            StartInfo = { FileName = BFdirectory + @"\bf3.exe" }
	            };
	            BFstart.Start();
            }
            catch {}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Process[] OriginKill = Process.GetProcessesByName("Origin");
				foreach (Process origin in OriginKill)
				{
					origin.Kill();
				}
            }
            catch {}

            try
            {
				Process[] BFkill = Process.GetProcessesByName("bf3");

				Process BFstart = new Process();
                BFstart.StartInfo = BFkill[0].StartInfo;

	            foreach (Process bf in BFkill)
	            {
					bf.Kill();
		            bf.WaitForExit();
	            }

                BFstart.Start();
            }
            catch {}
        }

        private void ReadCFG()
        {
            string[] lines = File.ReadAllLines(@BFdirectory + @"\user.cfg");
            foreach (string line in lines)
            {
                if (line.Contains("GameTime.MaxVariableFps"))
                {
                    string value = line.Replace("GameTime.MaxVariableFps ", "");

					string fps = value.Contains(".")
						? value.Remove(value.LastIndexOf('.'), value.Length - value.LastIndexOf('.'))
						: value;

					numericUpDown1.Value = Convert.ToDecimal(fps);
                }
                else if (line.Contains("RenderDevice.ForceRenderAheadLimit"))
                {
	                string value = line.Replace("RenderDevice.ForceRenderAheadLimit ", "");
					comboBox4.SelectedIndex = Convert.ToInt32(value) + 1;
                }

                else if (line.Contains("Render.DrawFPS 1")) checkBox1.Checked = true;
                else if (line.Contains("WorldRender.SpotlightShadowmapEnable 0")) checkBox2.Checked = false;
                else if (line.Contains("WorldRender.FxaaEnable 1")) checkBox3.Checked = true;
                else if (line.Contains("UI.DrawEnable 0")) checkBox4.Checked = false;
                else if (line.Contains("WorldRender.DxDeferredCsPathEnable 0")) checkBox5.Checked = false;
                else if (line.Contains("RenderDevice.Dx11Enable 1")) comboBox3.SelectedIndex = 1;
                else if (line.Contains("RenderDevice.Dx11Enable 0")) comboBox3.SelectedIndex = 0;
                else if (line.Contains("RenderDevice.TripleBufferingEnable 1")) checkBox7.CheckState = CheckState.Checked;
                else if (line.Contains("RenderDevice.TripleBufferingEnable 0")) checkBox7.CheckState = CheckState.Unchecked;
                else if (line.Contains("Render.PerfOverlayEnable 1") && line.Contains("Render.PerfOverlayVisible 1")) checkBox8.Checked = true;
                else if (line.Contains("WorldRender.TransparencyShadowmapsEnable 0")) checkBox9.Checked = false;

                else if (line.Contains("WorldRender.SpotLightShadowmapResolution 64")) comboBox2.SelectedIndex = 0;
                else if (line.Contains("WorldRender.SpotLightShadowmapResolution 256")) comboBox2.SelectedIndex = 1;
                else if (line.Contains("WorldRender.SpotLightShadowmapResolution 1024")) comboBox2.SelectedIndex = 2;
                else if (line.Contains("WorldRender.SpotLightShadowmapResolution 2048")) comboBox2.SelectedIndex = 3;
                else if (line.Contains("WorldRender.SpotLightShadowmapResolution 4096")) comboBox2.SelectedIndex = 4;
                else if (line.Contains("WorldRender.SpotLightShadowmapResolution 8192")) comboBox2.SelectedIndex = 5;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox2.Checked)
				comboBox2.SelectedIndex = 0;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (BFdirectory.Length == 0)
            {
                if (LoadPath())
					ReadCFG();
                else
					return;
            }

            if (!File.Exists(BFdirectory + @"\user.cfg"))
            {
                FileStream fs = File.Create(BFdirectory + @"\user.cfg");
                fs.Close();
            }

	        Process notepad = new Process
	        {
		        StartInfo =
		        {
			        FileName = "notepad.exe",
			        Arguments = BFdirectory + @"\user.cfg"
		        }
	        };
	        notepad.Start();
        }

        private void button7_Click(object sender, EventArgs e)
        {
	        Process explorer = new Process
	        {
		        StartInfo =
		        {
			        FileName = "explorer.exe",
			        Arguments = screenshotsPath
		        }
	        };
	        explorer.Start();
        }

        private int CheckDxVersion()
        {
	        Process dxdiag = new Process
	        {
		        StartInfo =
		        {
			        FileName = "dxdiag.exe",
					Arguments = "/t " + dxdiagOutput
		        }
	        };
	        dxdiag.Start();
            dxdiag.WaitForExit();

            if (File.Exists(Environment.GetEnvironmentVariable("temp") + @"\dxdiag.tmp"))
            {
				string[] lines = File.ReadAllLines(dxdiagOutput);
                foreach (string line in lines)
				{
					if (line.Contains("DirectX "))
					{
						if (line.Contains("DirectX 9")) return 9;
						if (line.Contains("DirectX 10")) return 10;
						if (line.Contains("DirectX 11")) return 11;
						if (line.Contains("DirectX 12")) return 11;
						if (line.Contains("DirectX 13")) return 11;
					}
				}
            }

            return 0;
        }

		private static string BoolToInt(bool value)
		{
			return (value) ? "1" : "0";
		}
    }
}
