﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IceBlink2
{
    public partial class IBHtmlMessageBox : IBFormGDI
    {
        public GameView gv;

        //from IbbHtmlLogBox
        public SolidBrush brush = new SolidBrush(Color.Black);
        public FontFamily fontfamily;
        public Font font;
        public Bitmap btn_up;
        public Bitmap btn_down;
        public Bitmap btn_scroll;
        public Bitmap bg_scroll;
        public List<string> tagStack = new List<string>();
        public List<FormattedLineGDI> logLinesList = new List<FormattedLineGDI>();
        public int currentTopLineIndex = 0;
        public int numberOfLinesToShow = 17;
        public int xLoc = 0;
        public int moveEY = 0;
        public int moveScrollingStartY = 0;
        public int scrollButtonYLoc = 0;
        public int startY = 0;
        public int moveDeltaY = 0;
        public int tbHeight = 200;
        public int tbWidth = 300;
        public int tbXloc = 10;
        public int tbYloc = 10;
        public float fontHeightToWidthRatio = 1.0f;

        public IBHtmlMessageBox(GameView g, string htmlstring)
        {
            InitializeComponent();            
            gv = g;
            button1.Text = "RETURN";
            button1.Font = gv.drawFontReg;
            this.KeyPreview = true;
            this.IceBlinkButtonClose.Enabled = false;
            this.IceBlinkButtonResize.Enabled = false;
            this.IceBlinkButtonClose.Visible = false;
            this.IceBlinkButtonResize.Visible = false;
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.form_MouseWheel);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.form_MouseMove);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.form_onKeyDown);
            //InitializeHtmlLogBox(10, 30,580 * (int)(100f/gv.squareSize), 400 * (int)(100f/gv.squareSize));

            //old teufel
            InitializeHtmlLogBox(10, 30, 780, 540);
            //int i = gv.screenWidth; //1366
            //int t = gv.screenHeight; //768
            //InitializeHtmlLogBox(10, 30, 13 * gv.squareSize, 10 *gv.squareSize);


            //InitializeHtmlLogBox(10, 30, 780 * (int)(100f / gv.squareSize), 400 * (int)(100f / gv.squareSize));

            //tried to adjust the size of the HtmlLogBox for my laptop resolution, tried to do in a dynamic wa
            //this one is still used when calling the info/tutorial ingame texts
            //InitializeHtmlLogBox(10, 30, (int)(580*(1920f/gv.Width) - (gv.squareSize/2)), (int)(400*(100f/gv.squareSize)));
            //InitializeHtmlLogBox(10, 30, (int)(580 * (1920f / gv.Width)), (int)(400 * (100f / gv.squareSize)));                                                                                                                            
            //InitializeHtmlLogBox(10, 30, (int)(580 * 100 /150 * (100f / gv.squareSize)), (int)(400 * 100 / 150 * (100f / gv.squareSize)));  
            AddHtmlTextToLog(htmlstring);
            numberOfLinesToShow = 17;
            AddHtmlTextToLog("");
            currentTopLineIndex = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Call the OnPaint method of the base class.
            base.OnPaint(e);
            onDrawLogBox(e.Graphics);
        }
        private void form_MouseWheel(object sender, MouseEventArgs e)
        {
            onMouseWheel(sender, e);
        }
        private void form_MouseMove(object sender, MouseEventArgs e)
        {
            onMouseMove(sender, e);
        }

        private void form_onKeyDown(object sender, KeyEventArgs e)
        {
            onKeyDown(sender, e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (keyData == Keys.Escape)            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }

            if ((keyData == Keys.Up) || (keyData == Keys.NumPad8))
            {
                float pageLength = 12f * (1080f / gv.screenHeight) + 1;
                if (gv.mod.fontName != "Metamorphous" && gv.mod.fontName != "Orbitron")
                {
                    float mod = (gv.screenHeight / 1080);
                    pageLength -= (int)mod;
                }
                //SetCurrentTopLineIndex((int)(-0.75f * 1080f / gv.screenHeight));
                SetCurrentTopLineIndex(-1);
                //SetCurrentTopLineIndex(-(int)pageLength);
                this.Invalidate();
                return true;
            }

            if ((keyData == Keys.Down)|| (keyData == Keys.NumPad2))
            {
                float pageLength = 12f * (1080f / gv.screenHeight) + 1;
                if (gv.mod.fontName != "Metamorphous" && gv.mod.fontName != "Orbitron")
                {
                    float mod = (gv.screenHeight / 1080);
                    pageLength -= (int)mod;
                }
                //SetCurrentTopLineIndex((int)(pageLength));
                //SetCurrentTopLineIndex((int)(0.75f*1080f / gv.screenHeight));
                SetCurrentTopLineIndex(1);
                //gv.Font.Height
                this.Invalidate();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            onMouseUp(sender, e);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
        
        public void ResizeForm()
        {
            int totalHeight = logLinesList.Count * font.Height + tbXloc + 50;
            if (totalHeight > 500)
            {
                this.Height = 500;
            }
            else
            {
                this.Height = totalHeight;
            }
            tbHeight = this.Height - tbXloc - 100;
            numberOfLinesToShow = (tbHeight / font.Height) - 7;
            //numberOfLinesToShow = (int)(1080f / gv.screenHeight);

        }

        //From IbbHtmlLogBox
        public void InitializeHtmlLogBox(int locX, int locY, int width, int height)
        {
            btn_up = gv.cc.LoadBitmapGDI("btn_up.png");
            btn_down = gv.cc.LoadBitmapGDI("btn_down.png");
            btn_scroll = gv.cc.LoadBitmapGDI("btn_scroll.png");
            bg_scroll = gv.cc.LoadBitmapGDI("bg_scroll.png");
            fontfamily = gv.family;
            font = new Font(fontfamily, 20.0f * (float)71f / 100.0f);
            tbXloc = locX;
            tbYloc = locY;
            tbWidth = width;
            tbHeight = height;
            brush.Color = Color.Red;
        }

        public void DrawBitmap(Graphics g, Bitmap bmp, int x, int y)
        {
            Rectangle src = new Rectangle(0, 0, bmp.Width, bmp.Height);
            Rectangle dst = new Rectangle(x + tbXloc, y + tbYloc, bmp.Width, bmp.Height);
            g.DrawImage(bmp, dst, src, GraphicsUnit.Pixel);
        }
        public void DrawString(Graphics g, string text, Font f, SolidBrush sb, int x, int y)
        {
            if ((y > -2) && (y <= tbHeight - f.Height))
            {
                g.DrawString(text, f, sb, x + tbXloc, y + tbYloc);
            }
        }

        public void AddHtmlTextToLog(string htmlText)
        {
            //Remove any '\r\n' hard returns from message
            htmlText = htmlText.Replace("\r\n", " ");
            htmlText = htmlText.Replace("\"", "'");

            if ((htmlText.EndsWith("<br>")) || (htmlText.EndsWith("<BR>")))
            {
                ProcessHtmlString(htmlText, tbWidth - btn_up.Width);
            }
            else
            {
                ProcessHtmlString(htmlText + "<br>", tbWidth - btn_up.Width);
            }            
            scrollToEnd();
        }
        public void ProcessHtmlString(string text, int width)
        {
            bool tagMode = false;
            string tag = "";
            FormattedWordGDI newWord = new FormattedWordGDI();
            FormattedLineGDI newLine = new FormattedLineGDI();
            int lineHeight = 0;

            foreach (char c in text)
            {
                #region Start/Stop Tags
                //start a tag and check for end of word
                if (c == '<')
                {
                    tagMode = true;

                    if (newWord.text != "")
                    {
                        newWord.fontStyle = GetFontStyle();
                        newWord.fontSize = GetFontSizeInPixels();
                        newWord.color = GetColor();
                        font = new Font(fontfamily, newWord.fontSize, newWord.fontStyle);
                        int wordWidth = (int)((font.Size / fontHeightToWidthRatio) * (float)newWord.text.Length);
                        if (font.Height > lineHeight) { lineHeight = font.Height; }
                        if (xLoc + wordWidth > (width * (1920f / gv.screenWidth))) //word wrap
                        {
                            //end last line and add it to the log
                            newLine.lineHeight = lineHeight;
                            logLinesList.Add(newLine);
                            //start a new line and add this word
                            newLine = new FormattedLineGDI();
                            newLine.wordsList.Add(newWord);
                            xLoc = 0;
                        }
                        else //no word wrap, just add word
                        {
                            newLine.wordsList.Add(newWord);
                        }
                        //instead of drawing, just add to line list 
                        xLoc += wordWidth;
                        newWord = new FormattedWordGDI();
                    }
                    continue;
                }
                //end a tag
                else if (c == '>')
                {
                    //check for ending type tag
                    if (tag.StartsWith("/"))
                    {
                        //if </>, remove corresponding tag from stack
                        string tagMinusSlash = tag.Substring(1);
                        if (tag.StartsWith("/font"))
                        {
                            for (int i = tagStack.Count - 1; i > 0; i--)
                            {
                                if (tagStack[i].StartsWith("font"))
                                {
                                    tagStack.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            tagStack.Remove(tagMinusSlash);
                        }
                    }
                    else
                    {
                        //check for line break
                        if ((tag.ToLower() == "br") || (tag == "BR"))
                        {
                            newWord.fontStyle = GetFontStyle();
                            newWord.fontSize = GetFontSizeInPixels();
                            newWord.color = GetColor();
                            //end last line and add it to the log
                            newLine.lineHeight = lineHeight;
                            logLinesList.Add(newLine);
                            //start a new line and add this word
                            newLine = new FormattedLineGDI();
                            //newLine.wordsList.Add(newWord);
                            xLoc = 0;
                        }
                        //else if <>, add this tag to the stack
                        tagStack.Add(tag);
                    }
                    tagMode = false;
                    tag = "";
                    continue;
                }
                #endregion

                #region Words
                if (!tagMode)
                {
                    if (c != ' ') //keep adding to word until hit a space
                    {
                        newWord.text += c;
                    }
                    else //hit a space so end word
                    {
                        newWord.fontStyle = GetFontStyle();
                        newWord.fontSize = GetFontSizeInPixels();
                        newWord.color = GetColor();
                        font = new Font(fontfamily, newWord.fontSize, newWord.fontStyle);
                        //line breaks did not work correctly on laptop, tried to dynamically force earlier break
                        int wordWidth = (int)((font.Size / fontHeightToWidthRatio) * (float)newWord.text.Length * (1920f/gv.Width));

                        if (font.Height > lineHeight) { lineHeight = font.Height; }
                        if (xLoc + wordWidth > (width * (1920f / gv.screenWidth))) //word wrap
                        {
                            //end last line and add it to the log
                            newLine.lineHeight = lineHeight;
                            logLinesList.Add(newLine);
                            //start a new line and add this word
                            newLine = new FormattedLineGDI();
                            newLine.wordsList.Add(newWord);
                            xLoc = 0;
                        }
                        else //no word wrap, just add word
                        {
                            newLine.wordsList.Add(newWord);
                        }
                        //instead of drawing, just add to line list 
                        xLoc += wordWidth;
                        newWord = new FormattedWordGDI();
                    }
                }
                else if (tagMode)
                {
                    tag += c;
                }
                #endregion
            }
        }

        public void onDrawLogBox(Graphics g)
        {
            //ratio of #lines to #pixels
            float ratio = (float)(logLinesList.Count) / (float)(tbHeight - btn_down.Height - btn_up.Height - btn_scroll.Height);
            //float ratio = (float)(logLinesList.Count) / (float)(tbHeight);
            if (ratio < 1.0f) { ratio = 1.0f; }
            if (moveDeltaY != 0)
            {
                int lineMove = (startY + moveDeltaY) * (int)ratio;
                lineMove = (int)(lineMove * 37f / 100f);
                SetCurrentTopLineAbsoluteIndex(lineMove);
                //SetCurrentTopLineIndex(lineMove);
            }
            //only draw lines needed to fill textbox
            int xLoc = 0;
            int yLoc = 0;
            int maxLines = currentTopLineIndex + numberOfLinesToShow + 5;
            if (maxLines > logLinesList.Count) { maxLines = logLinesList.Count; }
            for (int i = currentTopLineIndex; i < maxLines; i++)
            {
                if (i >= 0)
                {
                    //loop through each line and print each word
                    foreach (FormattedWordGDI word in logLinesList[i].wordsList)
                    {
                        //print each word and move xLoc
                        font = new Font(fontfamily, word.fontSize, word.fontStyle);
                        int wordWidth = (int)(g.MeasureString(word.text, font)).Width;
                        brush.Color = word.color;
                        int difYheight = logLinesList[i].lineHeight - font.Height;
                        DrawString(g, word.text, font, brush, xLoc, yLoc + difYheight);
                        xLoc += wordWidth;
                    }
                    xLoc = 0;
                    yLoc += logLinesList[i].lineHeight;
                }
            }

            //determine the scrollbutton location            
            //scrollButtonYLoc = (currentTopLineIndex / (int)ratio);
            //bali
            float rhelp1 = currentTopLineIndex;
            float rhelp2 = logLinesList.Count;
            float ratio2 = rhelp1/rhelp2;
            ratio2 *= 100;
            scrollButtonYLoc = (int)((ratio2 * (tbHeight - btn_down.Height - btn_scroll.Height))/100f);
            if (scrollButtonYLoc > tbHeight - btn_down.Height - btn_scroll.Height)
            {
                scrollButtonYLoc = tbHeight - btn_down.Height - btn_scroll.Height;
            }
            if (scrollButtonYLoc < 0 + btn_up.Height)
            {
                scrollButtonYLoc = 0 + btn_up.Height;
            }

            //draw scrollbar
            for (int y = 0; y < tbHeight - 10; y += 10)
            {
                DrawBitmap(g, bg_scroll, tbWidth - bg_scroll.Width - 5, y);
            }
            DrawBitmap(g, btn_up, tbWidth - btn_up.Width, 0);
            DrawBitmap(g, btn_down, tbWidth - btn_down.Width, tbHeight - btn_down.Height);
            DrawBitmap(g, btn_scroll, tbWidth - btn_scroll.Width - 1, scrollButtonYLoc);

            //draw border for debug info
            g.DrawRectangle(new Pen(Color.DimGray), new Rectangle(tbXloc, tbYloc, tbWidth, tbHeight));
        }
                
        public void scrollToEnd()
        {
            SetCurrentTopLineIndex(logLinesList.Count);
        }
        public void SetCurrentTopLineIndex(int changeValue)
        {
            currentTopLineIndex += changeValue;
            if (currentTopLineIndex > logLinesList.Count - numberOfLinesToShow -2)
            {
                //currentTopLineIndex = logLinesList.Count - numberOfLinesToShow;
                currentTopLineIndex = logLinesList.Count - numberOfLinesToShow -2;
            }
            if (currentTopLineIndex < 0)
            {
                currentTopLineIndex = 0;
            }            
        }
        public void SetCurrentTopLineAbsoluteIndex(int absoluteValue)
        {
            currentTopLineIndex = absoluteValue;
            if (currentTopLineIndex < 0)
            {
                currentTopLineIndex = 0;
            }
            if (currentTopLineIndex > logLinesList.Count - numberOfLinesToShow -2)
            {
                //currentTopLineIndex = logLinesList.Count - numberOfLinesToShow;
                currentTopLineIndex = logLinesList.Count - numberOfLinesToShow -2;
            }
        }
        
        private Color GetColor()
        {
            //will end up using the last color on the stack
            Color clr = Color.White;
            foreach (string s in tagStack)
            {
                if ((s == "font color='red'") || (s == "font color = 'red'"))
                {
                    clr = Color.Red;
                }
                else if ((s == "font color='lime'") || (s == "font color = 'lime'"))
                {
                    clr = Color.Lime;
                }
                else if ((s == "font color='black'") || (s == "font color = 'black'"))
                {
                    clr = Color.Black;
                }
                else if ((s == "font color='blue'") || (s == "font color = 'blue'"))
                {
                    clr = Color.Blue;
                }
                else if ((s == "font color='teal'") || (s == "font color = 'teal'"))
                {
                    clr = Color.Teal;
                }
                else if ((s == "font color='white'") || (s == "font color = 'white'"))
                {
                    clr = Color.White;
                }
                else if ((s == "font color='silver'") || (s == "font color = 'silver'"))
                {
                    clr = Color.Gray;
                }
                else if ((s == "font color='grey'") || (s == "font color = 'grey'"))
                {
                    clr = Color.DimGray;
                }
                else if ((s == "font color='aqua'") || (s == "font color = 'aqua'"))
                {
                    clr = Color.Aqua;
                }
                else if ((s == "font color='fuchsia'") || (s == "font color = 'fuchsia'"))
                {
                    clr = Color.Fuchsia;
                }
                else if ((s == "font color='yellow'") || (s == "font color = 'yellow'"))
                {
                    clr = Color.Yellow;
                }
            }
            return clr;
        }
        private FontStyle GetFontStyle()
        {
            FontStyle style = FontStyle.Regular;
            foreach (string s in tagStack)
            {
                if (s == "b")
                {
                    style = style | FontStyle.Bold;
                }
                else if (s == "i")
                {
                    style = style | FontStyle.Italic;
                }
                else if (s == "u")
                {
                    style = style | FontStyle.Underline;
                }
            }
            return style;
        }
        private float GetFontSizeInPixels()
        {
            //float fSize = 20.0f * (float)gv.squareSize / 100.0f;
            float fSize = 20.0f * (float)71f / 100.0f;
            foreach (string s in tagStack)
            {
                if (s == "big")
                {
                    fSize = 24.0f * (float)71f / 100.0f;
                }
                else if (s == "small")
                {
                    fSize = 16.0f * (float)71f  / 100.0f;
                }
            }
            return fSize;
        }

        private bool isMouseWithinTextBox(MouseEventArgs e)
        {
            if ((e.X > tbXloc) && (e.X < tbWidth + tbXloc) && (e.Y > tbYloc) && (e.Y < tbHeight + tbYloc))
            {
                return true;
            }
            return false;
        }
        private bool isMouseWithinScrollBar(MouseEventArgs e)
        {
            if ((e.X > tbWidth + tbXloc - btn_up.Width) && (e.X < tbWidth + tbXloc) && (e.Y > (tbYloc -100)) && (e.Y < tbHeight + tbYloc))
            {
                return true;
            }
            return false;
        }

        public void onMouseWheel(object sender, MouseEventArgs e)
        {
            if (isMouseWithinTextBox(e))
            {
                // Update the drawing based upon the mouse wheel scrolling. 
                int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

                if (numberOfTextLinesToMove > 0)
                {
                    SetCurrentTopLineIndex(-1 );
                    this.Invalidate();
                }
                if (numberOfTextLinesToMove < 0)
                {
                    SetCurrentTopLineIndex(1);
                    this.Invalidate();
                }
            }
        }
        public void onMouseDown(object sender, MouseEventArgs e)
        {
            //if (!isMouseWithinScrollBar(e))
            //{
                moveDeltaY = 0;
            //}

            //if (isMouseWithinScrollBar(e))
            //{

                //if (e.Y - tbYloc < scrollButtonYLoc)
                //{
                    //if mouse is above scroll button, move up a bit
                    //xLoc = 0;
                //}
                //else if (e.Y - tbYloc > scrollButtonYLoc + btn_scroll.Height)
                //{
                    //if mouse is below scroll button, move down a bit
                    //xLoc = 0;
                //}
                //else
                //{
                    //if mouse is on scroll button, set moveScrollingStartY = e.Y
                    moveScrollingStartY = e.Y;
                    startY = scrollButtonYLoc;
                //}
            //}
            //else if (isMouseWithinTextBox(e))
            //{
                //moveScrollingStartY = e.Y;
            //}
        }
        public void onMouseMove(object sender, MouseEventArgs e)
        {
            //if (isMouseWithinScrollBar(e))
            //{
                //if button is down and was on scroll button then move button and scroll text accordingly
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    moveEY = e.Y;
                    xLoc = 0;
                    moveDeltaY = (int)((gv.screenHeight/1080f) * (e.Y - moveScrollingStartY));
                    this.Invalidate();
                }
            //}
            //else if (isMouseWithinTextBox(e))
            //{
                //if (e.Button == System.Windows.Forms.MouseButtons.Left)
                //{
                    //this.Invalidate();
                //}
            //}
        }
        public void onMouseUp(object sender, MouseEventArgs e)
        {
            //if (!isMouseWithinScrollBar(e))
            //{
                moveDeltaY = 0;
            //}
            moveScrollingStartY = 0;
            if (isMouseWithinScrollBar(e))
            {
                //if click on top button, move 17 lines up
                if (e.Y < tbYloc + btn_up.Height)
                {
                    float pageLength = 12f * (1080f / gv.screenHeight) + 1;
                    if (gv.mod.fontName != "Metamorphous" && gv.mod.fontName != "Orbitron")
                    {
                        float mod = (gv.screenHeight/1080);
                        pageLength -= (int)mod;
                    }
                    SetCurrentTopLineIndex(-(int)pageLength);
                    this.Invalidate();
                }
                else if (e.Y > tbYloc + tbHeight - btn_down.Height)
                {
                    float pageLength = 12f * (1080f / gv.screenHeight) + 1;
                    if (gv.mod.fontName != "Metamorphous" && gv.mod.fontName != "Orbitron")
                    {
                        float mod = (gv.screenHeight / 1080);
                        pageLength -= (int)mod;
                    }
                    SetCurrentTopLineIndex((int)pageLength);
                    this.Invalidate();
                }
            }
            else if (isMouseWithinTextBox(e))
            {
                //this.Invalidate();
            }
        }

        public void onKeyboardEvent(Keys keyData)
        {
            //onKeyUp(keyData);
        }

        //not used actually, see the protected override override
        public void onKeyDown(Object sender, KeyEventArgs e)
        {
                if (e.KeyCode == Keys.Up)
                {
                SetCurrentTopLineIndex(-12);
                this.Invalidate();
            }

            if (e.KeyCode == Keys.Down)
            {
                SetCurrentTopLineIndex(12);
                this.Invalidate();
            }

        }
    }
}
