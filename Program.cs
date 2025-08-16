/*
* Kitt Scanner
* C.Winters / US / Arizona / Thinkpad T15g
* Nov 2024
* 
* Remember: If it doesn't work, I didn't write it.
* 
* Purpose: 
* Knight Rider -> Back in the 80's, the Knight Rider TV show featured a car with a scanner light that moved back and forth.
* Now you can have this in your own application. Many people hav done it, but this is my version.
* 
* Built with Visual Studio 2022 Enterprise (C# Lang.) and help with Copilot. DEbugging in VS with Copilot is pretty awesome.
* 
* 
* To build this project, you need to have .NET Framework 4.8 installed.
* Run 
* "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvars64.bat"
*
* This will set up the environment variables for building .NET Framework projects.
*   -  Note: In my case, I'm using VS2022 Enterprise, so the path may vary based on your VS version and installation.
*
*  msbuild /p:Configuration=Debug /p:Platform=x64
*  
*  Output will be in the bin\x64\Release folder.
*  
*/

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Kitt
{
    public class KnightRiderForm : Form
    {
        private Panel displayPanel;
        private Panel bottomPanel;
        private TrackBar trackBarSpeed;
        private TrackBar trackBarLEDCount;
        private CheckBox glowCheckBox;
        private Label sizeLabel;
        private Thread animationThread;
        private volatile bool running;
        private double scannerPosition;
        private double scannerStep;
        private readonly object locker = new object();
        
        // Number of LED segments
        private int ledCount =25;
        private bool enableInnerGlow = true;
        private double currentSpeed = 0.2;


        public KnightRiderForm()
        {
            Text = "KnightRider - KITT Scanner - Form";
            Size = new Size(562, 120);


            #region Form UI/UX Setup

            // Create display panel.
            //displayPanel = new Panel
            //{
            //    Dock = DockStyle.Fill,
            //    BackColor = Color.Black
            //};

            displayPanel = new DoubleBufferedPanel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.Black 
            };

            displayPanel.Paint += DisplayPanel_Paint;
            Controls.Add(displayPanel);

            // Create bottom panel to hold the TrackBar and size Label.
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            Controls.Add(bottomPanel);


            Label trackBarSpeedLabel = new Label
            {
                Text = $"Speed 0-10:",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(10, 5)
            };
            bottomPanel.Controls.Add(trackBarSpeedLabel);


            // TrackBar for speed control.
            trackBarSpeed = new TrackBar
            {
                Minimum = 0,
                Maximum = 10,
                // Gives a speed of 0.2 (Value / 10.0)
                Value = 2, 
                TickStyle = TickStyle.BottomRight,
                SmallChange = 1,
                LargeChange = 1,
                Location = new Point(trackBarSpeedLabel.Right + 5, 2)
            };
            trackBarSpeed.ValueChanged += TrackBarSpeed_ValueChanged;
            bottomPanel.Controls.Add(trackBarSpeed);


            //Insert label describing tracbarLedCount
            Label ledCountLabel = new Label
            {
                Text = $"LED Count (5-{ledCount}):",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(trackBarSpeed.Right + 5, 5)
            };
            bottomPanel.Controls.Add(ledCountLabel);

            // TrackBar for adding/removing LED segments.
            // If you lower it really low,you'll need to pause the animation thread to avoid issues with the scanner position!
            trackBarLEDCount = new TrackBar
            {
                Minimum = 5,
                Maximum = 25,
                Value = ledCount,
                SmallChange = 1,
                LargeChange = 1,
                Location = new Point(ledCountLabel.Right + 5, 2)
            };

            trackBarLEDCount.ValueChanged += (s, e) =>
            {
                //Update the ledCount
                ledCount = trackBarLEDCount.Value;
                // Redraw the display panel to reflect the new LED count.
                displayPanel.Invalidate();
            };
            bottomPanel.Controls.Add(trackBarLEDCount);



            // Create the label to display window size.
            sizeLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            bottomPanel.Controls.Add(sizeLabel);

            //Add a tickbox to toggle inner glow circle effect
            glowCheckBox = new CheckBox
               {
                    Text = "Inner Glow",
                    Checked = true,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Location = new Point(trackBarLEDCount.Right + 5, 5)
                };

            // Handle the CheckedChanged event to toggle the glow effect.
            // The s, e parameters are not used, but required by the event handler signature.
            glowCheckBox.CheckedChanged += (s, e) =>
            {
                // Toggle the glow effect based on the checkbox state and redraw the panel.
                displayPanel.Invalidate();
                enableInnerGlow = glowCheckBox.Checked;
            };
            bottomPanel.Controls.Add(glowCheckBox);



            //Subscribe to resize events to update the label.
            this.Resize += KnightRiderForm_Resize;
            bottomPanel.Resize += BottomPanel_Resize;

            #endregion Form Setup

            //Scanner parameters.
            scannerPosition = 0;
            scannerStep = currentSpeed;
            running = true;

            //Start the animation thread when the form is shown.
            this.Shown += KnightRiderForm_Shown;
            FormClosing += KnightRiderForm_FormClosing;
            
            // center screen the form
            StartPosition = FormStartPosition.CenterScreen;
        }


        /// <summary>
        /// Update the size label when the form is resized.  (typical form resize event)
        /// </summary>  
        private void KnightRiderForm_Resize(object sender, EventArgs e)
        {
            UpdateSizeLabel();
        }

        /// <summary>
        /// Update the size label when the form is resized. (bottompanel)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomPanel_Resize(object sender, EventArgs e)
        {
            UpdateSizeLabel();
        }

        // Helper to update the label text and position.
        private void UpdateSizeLabel()
        {
            sizeLabel.Text = $"Width: {this.ClientSize.Width}, Height: {this.ClientSize.Height}";
            
            // Position the label at the bottom right of the bottom panel with a margin.
            sizeLabel.Location = new Point(bottomPanel.Width - sizeLabel.Width - 10,
                                           (bottomPanel.Height - sizeLabel.Height) / 2);
        }


        /// <summary>
        /// Event handler for TrackBar value change, Adjusting speed based on the slider's value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrackBarSpeed_ValueChanged(object sender, EventArgs e)
        {
            lock (locker)
            {
                currentSpeed = trackBarSpeed.Value / 10.0;
                // Preserve the current direction.
                scannerStep = (scannerStep >= 0 ? 1 : -1) * currentSpeed;
            }
        }

        
        /// <summary>
        /// Stgart the animation thread when the form is shown. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KnightRiderForm_Shown(object sender, EventArgs e)
        {
            animationThread = new Thread(new ThreadStart(AnimateScanner))
            {
                IsBackground = true
            };
            animationThread.Start();
        }

        /// <summary>
        /// Stop the animation thread on form closing. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KnightRiderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            running = false;
            if (animationThread != null && animationThread.IsAlive) animationThread.Join();
        }

        /// <summary>
        /// The animation loop updates the scanner position and triggers a repaint.
        /// </summary>
        private void AnimateScanner()
        {
            while (running)
            {
                lock (locker)
                {
                    // Update scannerStep to reflect current speed while preserving direction.
                    scannerStep = (scannerStep >= 0 ? 1 : -1) * currentSpeed;
                    scannerPosition += scannerStep;
                    
                    // Reverse direction at the boundaries.
                    if (scannerPosition >= ledCount - 1 || scannerPosition <= 0)
                        scannerStep = -scannerStep;
                }

                // Use BeginInvoke to avoid blocking.
                if (displayPanel.IsHandleCreated && !displayPanel.IsDisposed)
                    displayPanel.BeginInvoke(new Action(() => displayPanel.Invalidate()));
                
                Thread.Sleep(30);
            }
        }

        /// <summary>
        /// A custom <see cref="Panel"/> control with double buffering enabled to reduce flicker during rendering.
        /// </summary>
        /// <remarks>This control is designed to improve rendering performance and visual smoothness by
        /// enabling double buffering and optimizing painting behavior. It is particularly useful for scenarios
        /// involving frequent or complex redraws, such as custom graphics or animations. Excellent suggestion from Copilot</remarks>
        public class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                // Enable double buffering and other styles to reduce flicker.
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
                this.UpdateStyles();
            }
        }


        /// <summary>
        /// Draw the LED segments with brightness based on distance from the scanner, and add a small glowing circle in the center of each block.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int margin = 10;
            int spacing = 5;
            int availableWidth = displayPanel.Width - 2 * margin;
            int ledWidth = (availableWidth - ((ledCount - 1) * spacing)) / ledCount;
            int ledHeight = displayPanel.Height - 2 * margin;

            lock (locker)
            {
                for (int i = 0; i < ledCount; i++)
                {
                    double distance = Math.Abs(i - scannerPosition);
                    
                    // Adjust denominator to control fade.
                    double intensity = Math.Max(0, 1 - (distance / 5.0)); 
                    int redComponent = (int)(intensity * 255);
                    Color ledColor = Color.FromArgb(redComponent, 0, 0);

                    int x = margin + i * (ledWidth + spacing);
                    Rectangle ledRect = new Rectangle(x, margin, ledWidth, ledHeight);
                    using (SolidBrush brush = new SolidBrush(ledColor))
                    {
                        g.FillRectangle(brush, ledRect);
                    }

                    // Draw a small glowing circle in the center of the block.
                    int circleDiameter = Math.Min(ledWidth, ledHeight) / 2;
                    int circleX = x + (ledWidth - circleDiameter) / 2;
                    int circleY = margin + (ledHeight - circleDiameter) / 2;


                    if (enableInnerGlow)
                    {
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddEllipse(circleX, circleY, circleDiameter, circleDiameter);
                            // Create a PathGradientBrush for the glowing effect.

                            if (circleDiameter > 0)
                            {
                                using (PathGradientBrush pgBrush = new PathGradientBrush(path))
                                {
                                    // Set center to a bright white with alpha based on intensity.
                                    int alpha = (int)(intensity * 255);
                                    pgBrush.CenterColor = Color.FromArgb(alpha, Color.White);
                                    
                                    // Set edges to be fully transparent.
                                    pgBrush.SurroundColors = new Color[] { Color.FromArgb(0, Color.White) };

                                    g.FillEllipse(pgBrush, circleX, circleY, circleDiameter, circleDiameter);
                                }
                            }
                        }

                    }

                }
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new KnightRiderForm());
        }

    }
}
