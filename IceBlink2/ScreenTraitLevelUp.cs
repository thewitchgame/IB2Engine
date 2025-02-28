﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Color = SharpDX.Color;

namespace IceBlink2
{
    public class ScreenTraitLevelUp 
    {

        //private gv.module gv.mod;
        //public bool screenInPcCreation = false;
        //public bool screenInCombat = false;

        private GameView gv;
	
	    private int traitSlotIndex = 0;
        public int traitToLearnIndex = 1;
        private int slotsPerPage = 20;
        
        //added(1)
        private int maxPages = 20;
        private int tknPageIndex = 0;

        private List<IbbButton> btnTraitSlots = new List<IbbButton>();

        //added(3)
        private IbbButton btnTokensLeft = null;
        private IbbButton btnTokensRight = null;
        private IbbButton btnPageIndex = null;

        private IbbButton btnHelp = null;
	    private IbbButton btnSelect = null;
        private IbbButton btnInfo = null;
        private IbbButton btnExit = null;
	    List<string> traitsToLearnTagsList = new List<string>();
	    public Player pc;
        public bool infoOnly = false; //set to true when called for info only
        private string stringMessageTraitLevelUp = "";
        private IbbHtmlTextBox description;

        List<TraitAllowed> backupTraitsAllowed = new List<TraitAllowed>();


        public ScreenTraitLevelUp(Module m, GameView g) 
	    {
		    //gv.mod = m;
		    gv = g;
		    setControlsStart();
		    pc = new Player();
		    stringMessageTraitLevelUp = gv.cc.loadTextToString("data/MessageTraitLevelUp.txt");
	    }
	
	    public void resetPC(bool info_only, Player p)
	    {
		    pc = p;
            infoOnly = info_only;
            traitToLearnIndex = 1;
        }

        public void sortTraitsForLevelUp(Player pc)
        {
               
                //clear 
                backupTraitsAllowed.Clear();
                List<string> traitsForLearningTags = new List<string>();
                List<TraitAllowed> traitsForLearning = new List<TraitAllowed>();

            if (!infoOnly)
            {
                //add the unknown available traits first
                traitsForLearningTags = pc.getTraitsToLearn(gv.mod);
                foreach (string s in traitsForLearningTags)
                {
                    foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                    {
                        if (ta.tag == s)
                        {
                            traitsForLearning.Add(ta);
                        }
                    }
                }

                //sort the unknwon, available traits
                int levelCounter = 0;
                while (traitsForLearning.Count > 0)
                {
                    for (int i = traitsForLearning.Count - 1; i >= 0; i--)
                    {
                        if (levelCounter == traitsForLearning[i].atWhatLevelIsAvailable)
                        {
                            backupTraitsAllowed.Add(traitsForLearning[i]);
                            traitsForLearning.RemoveAt(i);
                        }
                    }
                    levelCounter++;
                }

                //add the unkown, not yet available traits
                foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                {
                    bool notKnownYet = true;

                    //not hidden
                    if (!ta.allow || ta.needsSpecificTrainingToLearn)
                    {
                        //do not show the "hidden" traits that require special learning here
                        notKnownYet = false;
                    }

                    //not known
                    foreach (string s in pc.knownTraitsTags)
                    {
                        if (s == ta.tag)
                        {
                            notKnownYet = false;
                        }
                    }
                    //not just learned
                    foreach (string s in pc.learningTraitsTags)
                    {
                        if (s == ta.tag)
                        {
                            notKnownYet = false;
                        }
                    }

                    //not available
                    if (ta.atWhatLevelIsAvailable <= pc.classLevel)
                    {
                        Trait tr = gv.mod.getTraitByTag(ta.tag);
                        //not available(attribues, prequisite trait)
                        //attributes
                        if (checkAttributeRequirementsOfTrait(pc, tr))
                        {
                            //prerequisite traits
                            if (!tr.prerequisiteTrait.Equals("none"))
                            {
                                //requires prereq so check if you have it
                                if (pc.knownTraitsTags.Contains(tr.prerequisiteTrait) || pc.learningTraitsTags.Contains(tr.prerequisiteTrait))
                                {
                                    notKnownYet = false;
                                }
                            }
                            else
                            {
                                notKnownYet = false;
                            }
                        }
                        
                        //not already replaced
                        foreach (string s in pc.replacedTraitsOrSpellsByTag)
                        {
                            if (s == ta.tag)
                            {
                                notKnownYet = false;
                            }
                        }
                    }

                    if (notKnownYet)
                    {
                        //add
                        traitsForLearning.Add(ta);
                    }
                }

                //sort the unknwon, not yet available traits
                levelCounter = 0;
                while (traitsForLearning.Count > 0)
                {
                    for (int i = traitsForLearning.Count - 1; i >= 0; i--)
                    {
                        if (levelCounter == traitsForLearning[i].atWhatLevelIsAvailable)
                        {
                            backupTraitsAllowed.Add(traitsForLearning[i]);
                            traitsForLearning.RemoveAt(i);
                        }
                    }
                    levelCounter++;
                }

                //add the known traits
                foreach (string s in pc.knownTraitsTags)
                {
                    foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                    {
                        if (ta.tag == s)
                        {
                            traitsForLearning.Add(ta);
                        }
                    }
                }

                foreach (string s in pc.learningTraitsTags)
                {
                    foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                    {
                        if (ta.tag == s)
                        {
                            traitsForLearning.Add(ta);
                        }
                    }
                }

                //sort the known traits
                levelCounter = 0;
                while (traitsForLearning.Count > 0)
                {
                    for (int i = traitsForLearning.Count - 1; i >= 0; i--)
                    {
                        if (levelCounter == traitsForLearning[i].atWhatLevelIsAvailable)
                        {
                            backupTraitsAllowed.Add(traitsForLearning[i]);
                            traitsForLearning.RemoveAt(i);
                        }
                    }
                    levelCounter++;
                }

            }
            //info only
            //todo: adjust like above, sigh
            else
            {

                //add the known traits
                foreach (string s in pc.knownTraitsTags)
                {
                    foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                    {
                        if (ta.tag == s)
                        {
                            traitsForLearning.Add(ta);
                        }
                    }
                }

                /*
                foreach (string s in pc.learningTraitsTags)
                {
                    foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                    {
                        if (ta.tag == s)
                        {
                            traitsForLearning.Add(ta);
                        }
                    }
                }
                */

                //sort the known traits
                int levelCounter = 0;
                while (traitsForLearning.Count > 0)
                {
                    for (int i = traitsForLearning.Count - 1; i >= 0; i--)
                    {
                        if (levelCounter == traitsForLearning[i].atWhatLevelIsAvailable)
                        {
                            backupTraitsAllowed.Add(traitsForLearning[i]);
                            traitsForLearning.RemoveAt(i);
                        }
                    }
                    levelCounter++;
                }

                //add the unknown available traits first
                traitsForLearningTags = pc.getTraitsToLearn(gv.mod);
                foreach (string s in traitsForLearningTags)
                {
                    foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                    {
                        if (ta.tag == s)
                        {
                            traitsForLearning.Add(ta);
                        }
                    }
                }

                //sort the unknwon, available traits
                levelCounter = 0;
                while (traitsForLearning.Count > 0)
                {
                    for (int i = traitsForLearning.Count - 1; i >= 0; i--)
                    {
                        if (levelCounter == traitsForLearning[i].atWhatLevelIsAvailable)
                        {
                            backupTraitsAllowed.Add(traitsForLearning[i]);
                            traitsForLearning.RemoveAt(i);
                        }
                    }
                    levelCounter++;
                }


                //add the unkown, not yet available traits
                foreach (TraitAllowed ta in pc.playerClass.traitsAllowed)
                {
                    bool notKnownYet = true;

                    //not hidden
                    if (!ta.allow || ta.needsSpecificTrainingToLearn)
                    {
                        //do not show the "hidden" traits that require special learning here
                        notKnownYet = false;
                    }

                    //not known
                    foreach (string s in pc.knownTraitsTags)
                    {
                        if (s == ta.tag)
                        {
                            notKnownYet = false;
                        }
                    }
                    //not already replaced
                    foreach (string s in pc.replacedTraitsOrSpellsByTag)
                    {
                        if (s == ta.tag)
                        {
                            notKnownYet = false;
                        }
                    }

                    //not available
                    if (ta.atWhatLevelIsAvailable <= pc.classLevel)
                    {
                        Trait tr = gv.mod.getTraitByTag(ta.tag);
                        if (checkAttributeRequirementsOfTrait(pc, tr))
                        {
                            //prerequisite traits
                            if (!tr.prerequisiteTrait.Equals("none"))
                            {
                                //requires prereq so check if you have it
                                if (pc.knownTraitsTags.Contains(tr.prerequisiteTrait))
                                {
                                    notKnownYet = false;
                                }
                            }
                            else
                            {
                                notKnownYet = false;
                            }
                        }
                        
                    }

                    if (notKnownYet)
                    {
                        //add
                        traitsForLearning.Add(ta);
                    }
                }

                //sort the unknwon, not yet available traits
                levelCounter = 0;
                while (traitsForLearning.Count > 0)
                {
                    for (int i = traitsForLearning.Count - 1; i >= 0; i--)
                    {
                        if (levelCounter == traitsForLearning[i].atWhatLevelIsAvailable)
                        {
                            backupTraitsAllowed.Add(traitsForLearning[i]);
                            traitsForLearning.RemoveAt(i);
                        }
                    }
                    levelCounter++;
                }

            }
        }
	    public void setControlsStart()
	    {			
    	    int pW = (int)((float)gv.screenWidth / 100.0f);
		    int pH = (int)((float)gv.screenHeight / 100.0f);
		    int padW = gv.squareSize/6;

            description = new IbbHtmlTextBox(gv, 320, 100, 500, 300);
            //description = new IbbHtmlTextBox(gv, 3*gv.squareSize + 2*pW, 2*gv.squareSize, gv.squareSize*5, gv.squareSize*10);
            description.showBoxBorder = false;

            //added
            if (btnTokensLeft == null)
            {
                btnTokensLeft = new IbbButton(gv, 1.0f);
                btnTokensLeft.Img = gv.cc.LoadBitmap("btn_small"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small);
                btnTokensLeft.Img2 = gv.cc.LoadBitmap("ctrl_left_arrow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.ctrl_left_arrow);
                btnTokensLeft.Glow = gv.cc.LoadBitmap("btn_small_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_glow);
                btnTokensLeft.X = (int)(5 * gv.squareSize) + (3 * pW);
                btnTokensLeft.Y = (1 * gv.squareSize);
                btnTokensLeft.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnTokensLeft.Width = (int)(gv.ibbwidthR * gv.screenDensity);
            }
            //added
            if (btnPageIndex == null)
            {
                btnPageIndex = new IbbButton(gv, 1.0f);
                btnPageIndex.Img = gv.cc.LoadBitmap("btn_small_off"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_off);
                btnPageIndex.Glow = gv.cc.LoadBitmap("btn_small_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_glow);
                btnPageIndex.Text = "1/20";
                btnPageIndex.X = (int)(6 * gv.squareSize) + (3 * pW);
                btnPageIndex.Y = (1 * gv.squareSize);
                btnPageIndex.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnPageIndex.Width = (int)(gv.ibbwidthR * gv.screenDensity);
            }
            //added
            if (btnTokensRight == null)
            {
                btnTokensRight = new IbbButton(gv, 1.0f);
                btnTokensRight.Img = gv.cc.LoadBitmap("btn_small"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small);
                btnTokensRight.Img2 = gv.cc.LoadBitmap("ctrl_right_arrow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.ctrl_right_arrow);
                btnTokensRight.Glow = gv.cc.LoadBitmap("btn_small_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_glow);
                btnTokensRight.X = (int)(7f * gv.squareSize) + (3 * pW);
                btnTokensRight.Y = (1 * gv.squareSize);
                btnTokensRight.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnTokensRight.Width = (int)(gv.ibbwidthR * gv.screenDensity);
            }

            if (btnSelect == null)
		    {
			    btnSelect = new IbbButton(gv, 0.8f);	
			    btnSelect.Text = "LEARN SELECTED CHOICE";
			    btnSelect.Img = gv.cc.LoadBitmap("btn_large"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_large);
			    btnSelect.Glow = gv.cc.LoadBitmap("btn_large_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_large_glow);
                btnSelect.X = (gv.screenWidth / 2) - (int)(gv.ibbwidthL * gv.screenDensity / 2.0f);
			    btnSelect.Y = 9 * gv.squareSize + pH * 2;
                btnSelect.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnSelect.Width = (int)(gv.ibbwidthL * gv.screenDensity);			
		    }

            if (btnInfo == null)
            {
                btnInfo = new IbbButton(gv, 0.8f);
                btnInfo.Text = "MORE";
                btnInfo.Img = gv.cc.LoadBitmap("btn_small"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small);
                btnInfo.Glow = gv.cc.LoadBitmap("btn_small_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_glow);
                btnInfo.X = (15 * gv.squareSize) - padW * 1 + gv.oXshift;
                btnInfo.Y = 9 * gv.squareSize + pH * 2;
                btnInfo.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnInfo.Width = (int)(gv.ibbwidthR * gv.screenDensity);
            }

            if (btnHelp == null)
		    {
			    btnHelp = new IbbButton(gv, 0.8f);	
			    btnHelp.Text = "HELP";
			    btnHelp.Img = gv.cc.LoadBitmap("btn_small"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small);
			    btnHelp.Glow = gv.cc.LoadBitmap("btn_small_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_glow);
			    btnHelp.X = 5 * gv.squareSize + padW * 1 + gv.oXshift;
			    btnHelp.Y = 9 * gv.squareSize + pH * 2;
                btnHelp.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnHelp.Width = (int)(gv.ibbwidthR * gv.screenDensity);			
		    }
		    if (btnExit == null)
		    {
			    btnExit = new IbbButton(gv, 0.8f);	
			    btnExit.Text = "EXIT";
			    btnExit.Img = gv.cc.LoadBitmap("btn_small"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small);
			    btnExit.Glow = gv.cc.LoadBitmap("btn_small_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_glow);
			    btnExit.X = (15 * gv.squareSize) - padW * 1 + gv.oXshift;
			    btnExit.Y = 9 * gv.squareSize + pH * 2;
                btnExit.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnExit.Width = (int)(gv.ibbwidthR * gv.screenDensity);			
		    }
		    for (int y = 0; y < slotsPerPage; y++)
		    {
			    IbbButton btnNew = new IbbButton(gv, 1.0f);
                gv.cc.DisposeOfBitmap(ref btnNew.Img);
                btnNew.Img = gv.cc.LoadBitmap("btn_small"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small);
                gv.cc.DisposeOfBitmap(ref btnNew.Glow);
                btnNew.Glow = gv.cc.LoadBitmap("btn_small_glow"); // BitmapFactory.decodeResource(gv.getResources(), R.drawable.btn_small_glow);
			
			    int x = y % 5;
			    int yy = y / 5;
			    btnNew.X = ((x + 4) * gv.squareSize) + (padW * (x+1)) + gv.oXshift;
			    btnNew.Y = (2 + yy) * gv.squareSize + (padW * yy + padW);

                btnNew.Height = (int)(gv.ibbheight * gv.screenDensity);
                btnNew.Width = (int)(gv.ibbwidthR * gv.screenDensity);
			    btnTraitSlots.Add(btnNew);
		    }			
	    }

        //new method for checking attribute requiremnts of traits
        public bool checkAttributeRequirementsOfTrait (Player pc, Trait t)
        {
            gv.sf.UpdateStats(pc);

            if (pc.strength < t.requiredStrength)
            {
                return false;
            }
            if (pc.dexterity < t.requiredDexterity)
            {
                return false;
            }
            if (pc.constitution < t.requiredConstitution)
            {
                return false;
            }
            if (pc.intelligence < t.requiredIntelligence)
            {
                return false;
            }
            if (pc.wisdom < t.requiredWisdom)
            {
                return false;
            }
            if (pc.charisma < t.requiredCharisma)
            {
                return false;
            }

            return true;
        }
	
	    //CAST SELECTOR SCREEN (COMBAT and MAIN)
        public void redrawTraitLevelUp(bool inPcCreation)
        {
    	    traitsToLearnTagsList.Clear();
    	    fillToLearnList();
    	
    	    int pW = (int)((float)gv.screenWidth / 100.0f);
		    int pH = (int)((float)gv.screenHeight / 100.0f);
		
    	    int locY = 0;
    	    int locX = pW * 4;
            int textH = (int)gv.drawFontRegHeight;
    	    int spacing = textH;
            int tabX = 5 * gv.squareSize + pW * 3;
            int noticeX = 5 * gv.squareSize + pW * 3;
    	    int noticeY = pH * 1 + spacing;
    	    int tabStartY = 4 * gv.squareSize + pW * 10;

            if (!infoOnly)
            {
                
                
                //DRAW TEXT		
                locY = (gv.squareSize * 0) + (pH * 2);
                //gv.DrawText("Select One Trait to Learn", noticeX, pH * 1, 1.0f, Color.Gray);
                int maxNumber = 0;
                if (gv.mod.getPlayerClass(pc.classTag).traitsToLearnAtLevelTable[pc.classLevel] > pc.getTraitsToLearn(gv.mod).Count)
                {
                    maxNumber = pc.getTraitsToLearn(gv.mod).Count;
                    maxNumber += traitToLearnIndex-1;
                }
                else
                {
                    maxNumber = gv.mod.getPlayerClass(pc.classTag).traitsToLearnAtLevelTable[pc.classLevel];
                }
                gv.DrawText("Select Choice Nr. " + traitToLearnIndex + " of " + maxNumber + " Choice(s) to Learn", noticeX, pH * 1, 1.0f, Color.Gray);

                //DRAW NOTIFICATIONS
                if (isSelectedTraitSlotInKnownTraitsRange())
                {
                    Trait tr = GetCurrentlySelectedTrait();
                    
                    //check to see if already known
                    //if (pc.knownTraitsTags.Contains(tr.tag))
                    if ((pc.knownTraitsTags.Contains(tr.tag)) || (pc.learningTraitsTags.Contains(tr.tag)))
                    {
                        //say that you already know this one
                        gv.DrawText("Already Known", noticeX, noticeY, 1.0f, Color.Yellow);
                    }
                    else //trait not known
                    {
                        //checking attribute requiremnts of trait
                        bool attributeRequirementsMet = checkAttributeRequirementsOfTrait(pc, tr);
                        
                        //check if available to learn
                        if (isAvailableToLearn(tr.tag) && attributeRequirementsMet)
                        {
                            gv.DrawText("Available to Learn", noticeX, noticeY, 1.0f, Color.Lime);
                        }
                        else //not available yet
                        {
                            if (attributeRequirementsMet)
                            {
                                gv.DrawText(pc.playerClass.traitLabelSingular + " Not Available to Learn Yet", noticeX, noticeY, 1.0f, Color.Red);
                            }
                            else 
                            {
                                gv.DrawText("Attribute requirements not met", noticeX, noticeY, 1.0f, Color.Red);
                            }
                        }
                    }
                }
            }	
		
		    //DRAW ALL TRAIT SLOTS		
		    int cntSlot = 0;
		    foreach (IbbButton btn in btnTraitSlots)
		    {			
			    //Player pc = getCastingPlayer();
			
			    if (cntSlot == traitSlotIndex) {btn.glowOn = true;}
			    else {btn.glowOn = false;}

                //added
                //if ((cntSlot + (tknPageIndex * slotsPerPage)) < playerTokenList.Count)
                //{
                //}
                //show only traits for the PC class
                
                //here insert
                //do we need to recalculate without change?
                //sortTraitsForLevelUp(pc);

                //if ((cntSlot +(tknPageIndex * slotsPerPage)) < pc.playerClass.traitsAllowed.Count)
                if ((cntSlot + (tknPageIndex * slotsPerPage)) < backupTraitsAllowed.Count)
                {
                    //TraitAllowed ta = pc.playerClass.traitsAllowed[cntSlot + (tknPageIndex * slotsPerPage)];
                    TraitAllowed ta = backupTraitsAllowed[cntSlot + (tknPageIndex * slotsPerPage)];
                    Trait tr = gv.mod.getTraitByTag(ta.tag);

                    if (infoOnly)
                    {
                        if (pc.knownTraitsTags.Contains(tr.tag)) //check to see if already known, if so turn on button
                        {
                            gv.cc.DisposeOfBitmap(ref btn.Img);
                            btn.Img = gv.cc.LoadBitmap("btn_small");
                            gv.cc.DisposeOfBitmap(ref btn.Img2);
                            btn.Img2 = gv.cc.LoadBitmap(tr.traitImage);
                            gv.cc.DisposeOfBitmap(ref btn.Img3);
                            btn.Img3 = null;
                            //btn.Img3 = null;

                            //gv.cc.DisposeOfBitmap(ref btn.Img3);
                            //btn.Img3 = gv.cc.LoadBitmap("mandatory_conversation_indicator");
                        }
                        else //trait not known yet
                        {
                            /*
                            gv.cc.DisposeOfBitmap(ref btn.Img);
                            btn.Img = gv.cc.LoadBitmap("btn_small_off");
                            gv.cc.DisposeOfBitmap(ref btn.Img2);
                            btn.Img2 = gv.cc.LoadBitmap(tr.traitImage + "_off");
                            btn.Img3 = null;
                            */
                            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                            //checking attribute requiremnts of trait
                            bool attributeRequirementsMet = checkAttributeRequirementsOfTrait(pc, tr);
                            //if (tr.tag == "bluff")
                            //{
                            //int hghg = 0;
                            //}
                            if (isAvailableToLearn(tr.tag) && attributeRequirementsMet) //if available to learn, turn on button
                            {
                                gv.cc.DisposeOfBitmap(ref btn.Img);
                                btn.Img = gv.cc.LoadBitmap("btn_small_off");
                                gv.cc.DisposeOfBitmap(ref btn.Img2);
                                btn.Img2 = gv.cc.LoadBitmap(tr.traitImage + "_off");
                                gv.cc.DisposeOfBitmap(ref btn.Img3);
                                btn.Img3 = gv.cc.LoadBitmap("yellow_frame");
                            }
                            else //not available to learn, turn off button
                            {
                                gv.cc.DisposeOfBitmap(ref btn.Img);
                                btn.Img = gv.cc.LoadBitmap("btn_small_off");
                                gv.cc.DisposeOfBitmap(ref btn.Img2);
                                btn.Img2 = gv.cc.LoadBitmap(tr.traitImage + "_off");
                                gv.cc.DisposeOfBitmap(ref btn.Img3);
                                btn.Img3 = gv.cc.LoadBitmap("red_frame");
                            }

                            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                        }
                    }
                    else
                    {
                        if (pc.knownTraitsTags.Contains(tr.tag)) //check to see if already known, if so turn off button
                        {
                            gv.cc.DisposeOfBitmap(ref btn.Img);
                            btn.Img = gv.cc.LoadBitmap("btn_small_off");
                            gv.cc.DisposeOfBitmap(ref btn.Img2);
                            btn.Img2 = gv.cc.LoadBitmap(tr.traitImage + "_off");
                            gv.cc.DisposeOfBitmap(ref btn.Img3);
                            btn.Img3 = gv.cc.LoadBitmap("yellow_frame");
                        }
                        else //trait not known yet
                        {
                            //checking attribute requiremnts of trait
                            bool attributeRequirementsMet = checkAttributeRequirementsOfTrait(pc, tr);
                            //if (tr.tag == "bluff")
                            //{
                                //int hghg = 0;
                            //}
                            if (isAvailableToLearn(tr.tag) && attributeRequirementsMet) //if available to learn, turn on button
                            {
                                gv.cc.DisposeOfBitmap(ref btn.Img);
                                btn.Img = gv.cc.LoadBitmap("btn_small");
                                gv.cc.DisposeOfBitmap(ref btn.Img2);
                                btn.Img2 = gv.cc.LoadBitmap(tr.traitImage);
                                gv.cc.DisposeOfBitmap(ref btn.Img3);
                                btn.Img3 = null;
                                //btn.Img3 = null;
                            }
                            else //not available to learn, turn off button
                            {
                                gv.cc.DisposeOfBitmap(ref btn.Img);
                                btn.Img = gv.cc.LoadBitmap("btn_small_off");
                                gv.cc.DisposeOfBitmap(ref btn.Img2);
                                btn.Img2 = gv.cc.LoadBitmap(tr.traitImage + "_off");
                                gv.cc.DisposeOfBitmap(ref btn.Img3);
                                btn.Img3 = gv.cc.LoadBitmap("red_frame");
                            }
                        }
                    }				
			    }
			    else //slot is not in traits allowed index range
			    {
                    gv.cc.DisposeOfBitmap(ref btn.Img);
                    btn.Img = gv.cc.LoadBitmap("btn_small_off"); 
				    btn.Img2 = null;
                    btn.Img3 = null;
			    }			
			    btn.Draw();
			    cntSlot++;
		    }
		
		    //DRAW DESCRIPTION BOX
		    locY = tabStartY;		
		    if (isSelectedTraitSlotInKnownTraitsRange())
		    {
                Trait tr = GetCurrentlySelectedTrait();
                Spell sp = new Spell();
                if (tr.associatedSpellTag != "none" && tr.associatedSpellTag != "None" && tr.associatedSpellTag != "")
                {
                    sp = gv.mod.getSpellByTag(tr.associatedSpellTag);
                }
                //string textToSpan = "<u>Description</u>" + "<BR>" + "<BR>";
                string textToSpan = "<b><big>" + tr.name + "</big></b><BR>";
                textToSpan += "Available at Level: " + getLevelAvailable(tr.tag) + "<BR>";
                if (tr.associatedSpellTag != "none" && tr.associatedSpellTag != "None" && tr.associatedSpellTag != "")
                {
                    if (sp.isSwiftAction && !sp.usesTurnToActivate)
                    {
                        textToSpan += "Swift action" + "<BR>";
                    }
                    else if (sp.onlyOncePerTurn && !sp.usesTurnToActivate)
                    {
                        textToSpan += "Free action, not repeatable" + "<BR>";
                    }
                    else if (!sp.onlyOncePerTurn && !sp.usesTurnToActivate)
                    {
                        textToSpan += "Free action, repeatable" + "<BR>";
                    }
                    else if (sp.castTimeInTurns > 0)
                    {
                        textToSpan += "Takes " + sp.castTimeInTurns + " full turn(s)" + "<BR>";
                    }
                }
                if (sp.coolDownTime > 0)
                {
                    textToSpan += "Cool down time: " + sp.coolDownTime + " turn(s)" + "<BR>";
                }

                if (tr.prerequisiteTrait != "none")
                {
                    foreach (Trait t in gv.mod.moduleTraitsList)
                    {
                        if (t.tag == tr.prerequisiteTrait)
                        {
                            textToSpan += "Requires: " + t.name + "<BR>";
                            break;
                        }
                    }
                }

                if (tr.requiredStrength > 0)
                {
                    textToSpan += "Required STR: " + tr.requiredStrength + "<BR>"; 
                }
                if (tr.requiredDexterity > 0)
                {
                    textToSpan += "Required DEX: " + tr.requiredDexterity + "<BR>";
                }
                if (tr.requiredConstitution > 0)
                {
                    textToSpan += "Required CON: " + tr.requiredConstitution + "<BR>";
                }
                if (tr.requiredIntelligence > 0)
                {
                    textToSpan += "Required INT: " + tr.requiredIntelligence + "<BR>";
                }
                if (tr.requiredWisdom > 0)
                {
                    textToSpan += "Required WIS: " + tr.requiredWisdom + "<BR>";
                }
                if (tr.requiredCharisma > 0)
                {
                    textToSpan += "Required CHA: " + tr.requiredCharisma + "<BR>";
                }
                textToSpan += "<BR>";
                textToSpan += tr.description;

                description.tbXloc = 11 * gv.squareSize;
                description.tbYloc = 1 * gv.squareSize;
                description.tbWidth = pW * 40;
                description.tbHeight = pH * 80;
                description.logLinesList.Clear();
                description.AddHtmlTextToLog(textToSpan);
                description.onDrawLogBox();
		    }

            if (infoOnly)
            {
                btnInfo.Text = "MORE";
                btnSelect.Text = "RETURN";
                btnInfo.Draw();
                btnSelect.Draw();
                btnTokensLeft.Draw();
                btnTokensRight.Draw(); 
                btnPageIndex.Draw();
        //todo draw other buttons
        //zeke2
    }
            else
            {
                btnSelect.Text = "LEARN SELECTED " + pc.playerClass.traitLabelSingular.ToUpper();
                btnHelp.Draw();
                btnExit.Draw();
                btnSelect.Draw();
                btnInfo.Draw();
                //todo draw other buttons
                btnTokensLeft.Draw();
                btnTokensRight.Draw();
                btnPageIndex.Draw();
            }
        }
        public void onTouchTraitLevelUp(MouseEventArgs e, MouseEventType.EventType eventType, bool inPcCreation, bool inCombat)
	    {
            try
            {
                btnInfo.glowOn = false;
                btnHelp.glowOn = false;
                btnExit.glowOn = false;
                btnSelect.glowOn = false;
                btnTokensLeft.glowOn = false;
                btnTokensRight.glowOn = false;
                btnPageIndex.glowOn = false;

                switch (eventType)
                {
                    case MouseEventType.EventType.MouseDown:
                    case MouseEventType.EventType.MouseMove:
                        int x = (int)e.X;
                        int y = (int)e.Y;
                        if (btnHelp.getImpact(x, y))
                        {
                            btnHelp.glowOn = true;
                        }
                        else if (btnSelect.getImpact(x, y))
                        {
                            btnSelect.glowOn = true;
                        }
                        else if (btnInfo.getImpact(x, y))
                        {
                            btnInfo.glowOn = true;
                        }
                        else if (btnExit.getImpact(x, y))
                        {
                            btnExit.glowOn = true;
                        }
                        else if (btnTokensLeft.getImpact(x, y))
                        {
                            btnTokensLeft.glowOn = true;
                        }
                        else if (btnTokensRight.getImpact(x, y))
                        {
                            btnTokensRight.glowOn = true;
                        }
                        else if (btnPageIndex.getImpact(x, y))
                        {
                            btnPageIndex.glowOn = true;
                        }
                        break;

                    case MouseEventType.EventType.MouseUp:
                        x = (int)e.X;
                        y = (int)e.Y;

                        btnTokensLeft.glowOn = false;
                        btnTokensRight.glowOn = false;
                        btnHelp.glowOn = false;
                        btnExit.glowOn = false;
                        btnSelect.glowOn = false;
                        btnInfo.glowOn = false;

                        for (int j = 0; j < slotsPerPage; j++)
                        {
                            if (btnTraitSlots[j].getImpact(x, y))
                            {
                                gv.PlaySound("btn_click");
                                traitSlotIndex = j;
                            }
                        }
                        if (btnTokensLeft.getImpact(x, y))
                        {
                            if (tknPageIndex > 0)
                            {
                                tknPageIndex--;
                                btnPageIndex.Text = (tknPageIndex + 1) + "/" + maxPages;
                            }
                        }
                        else if (btnTokensRight.getImpact(x, y))
                        {
                            if (tknPageIndex < maxPages)
                            {
                                tknPageIndex++;
                                btnPageIndex.Text = (tknPageIndex + 1) + "/" + maxPages;
                            }
                        }
                        else if (btnHelp.getImpact(x, y))
                        {
                            if (!infoOnly)
                            {
                                gv.PlaySound("btn_click");
                                tutorialMessageTraitScreen();
                            }
                        }
                        else if (btnSelect.getImpact(x, y))
                        {
                            gv.PlaySound("btn_click");
                            if (infoOnly)
                            {
                                if (inCombat)
                                {
                                    gv.screenType = "combatParty";
                                }
                                else
                                {
                                    gv.screenType = "party";
                                }
                            }
                            else
                            {
                                doSelectedTraitToLearn(inPcCreation);
                            }
                        }
                        else if (btnInfo.getImpact(x, y))
                        {
                            string textToSpan = GetCurrentlySelectedTrait().description;
                            gv.sf.MessageBoxHtml(textToSpan);
                        }
                        else if (btnExit.getImpact(x, y))
                        {
                            if (!infoOnly)
                            {
                                gv.screenParty.traitGained = "";
                                gv.screenParty.spellGained = "";
                                pc.learningTraitsTags.Clear();
                                pc.learningEffects.Clear();
                                pc.learningSpellsTags.Clear();
                                traitToLearnIndex = 1;
                                gv.PlaySound("btn_click");

                                /*
                                if (inPcCreation)
                                {
                                    gv.screenType = "pcCreation";
                                }
                                else
                                {
                                    pc.classLevel--;
                                    gv.screenType = "party";
                                }
                                */
                                if (inPcCreation)
                                {
                                    gv.screenType = "pcCreation";
                                }
                                else //differentiate for combat, use incombat
                                {
                                    if (inCombat)
                                    {
                                        pc.classLevel--;
                                        gv.screenType = "combatParty";
                                    }
                                    else
                                    {
                                        pc.classLevel--;
                                        gv.screenType = "party";
                                    }
                                }
                            }

                            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                            else //differentiate for combat, use incombat
                            {
                                if (inCombat)
                                {
                                    gv.screenType = "combatParty";
                                }
                                else
                                {
                                    gv.screenType = "party";
                                }

                            }
                            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                        }
                        break;
                }
            }
            catch
            { }
	    }
    
        public void doSelectedTraitToLearn(bool inPcCreation)
        {
    	    if (isSelectedTraitSlotInKnownTraitsRange())
		    {
			    Trait tr = GetCurrentlySelectedTrait();

                //checking attribute requiremnts of trait
                bool attributeRequirementsMet = checkAttributeRequirementsOfTrait(pc, tr);

			    if (isAvailableToLearn(tr.tag) && attributeRequirementsMet)
			    {
                    //add trait
                    //pc.knownTraitsTags.Add(tr.tag);
                    pc.learningTraitsTags.Add(tr.tag);
                      
                    foreach (EffectTagForDropDownList etfddl in tr.traitEffectTagList)
                    {
                        foreach (Effect e in gv.mod.moduleEffectsList)
                        {
                            if (e.tag == etfddl.tag)
                            {
                                if (e.isPermanent)
                                {
                                    pc.learningEffects.Add(e);
                                }
                            }
                        }
                    }

                    sortTraitsForLevelUp(pc);

                    //check to see if there are more traits to learn at this level  
                    traitToLearnIndex++;
                    int maxNumber = 0;
                    if (gv.mod.getPlayerClass(pc.classTag).traitsToLearnAtLevelTable[pc.classLevel] > pc.getTraitsToLearn(gv.mod).Count)
                    {
                        maxNumber = pc.getTraitsToLearn(gv.mod).Count + 1;
                    }
                    else
                    {
                        maxNumber = gv.mod.getPlayerClass(pc.classTag).traitsToLearnAtLevelTable[pc.classLevel];
                    }
                    if (traitToLearnIndex <= maxNumber)
                    {
                        gv.screenParty.traitGained += tr.name + ", ";
                    }
                    else //finished learning all traits available for this level  
                    {
                        //else if in creation go back to partybuild				  
                        if (inPcCreation)
                        {
                            //if there are spells to learn go to spell screen next  
                            List < string > spellTagsList = new List<string>();
                            spellTagsList = pc.getSpellsToLearn();
                            if (spellTagsList.Count > 0)
                            {
                                gv.screenSpellLevelUp.resetPC(false, pc, false);
                                gv.screenType = "learnSpellCreation";
                            }
                            else //no spells to learn  
                            {
                                //save character, add them to the pcList of screenPartyBuild, and go back to build screen
                                //foreach (string s in pc.learningTraitsTags)
                                for (int counter = pc.learningTraitsTags.Count-1; counter >= 0; counter--)
                                {
                                    pc.knownTraitsTags.Add(pc.learningTraitsTags[counter]);
                                    //TODO: must get trait by ts string
                                    foreach (Trait t in gv.mod.moduleTraitsList)
                                    {
                                        if (t.tag == pc.learningTraitsTags[counter])
                                        {
                                            tr = t;
                                            break;
                                        }
                                    }

                                    //********************************************
                                    #region replacement code traits
                                    //get the trait tor replace (if existent)
                                    Trait temp2 = new Trait();
                                    foreach (Trait t in gv.mod.moduleTraitsList)
                                    {
                                        if (t.tag == tr.traitToReplaceByTag)
                                        {
                                            temp2 = t.DeepCopy();
                                        }
                                    }
                                    if ((tr.traitToReplaceByTag != "none") && (tr.traitToReplaceByTag != ""))
                                    {
                                        pc.replacedTraitsOrSpellsByTag.Add(tr.traitToReplaceByTag);
                                    }

                                    if (tr.traitToReplaceByTag != tr.prerequisiteTrait)
                                    {
                                        string replacedTag = tr.traitToReplaceByTag;
                                        for (int j = gv.mod.moduleTraitsList.Count - 1; j >= 0; j--)
                                        {
                                            if (gv.mod.moduleTraitsList[j].prerequisiteTrait == replacedTag)
                                            {
                                                if (!pc.replacedTraitsOrSpellsByTag.Contains(replacedTag))
                                                {
                                                    pc.replacedTraitsOrSpellsByTag.Add(gv.mod.moduleTraitsList[j].tag);
                                                    replacedTag = gv.mod.moduleTraitsList[j].tag;
                                                    j = gv.mod.moduleTraitsList.Count - 1;
                                                }
                                            }
                                        }
                                    }
                                    //adding trait to replace mechanism: known traits
                                    for (int i = pc.knownTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            //TODO: remove connected permannent effects
                                            //Peter
                                            for (int j = pc.effectsList.Count - 1; j >= 0; j--)
                                            {
                                                foreach (EffectTagForDropDownList etfddl in temp2.traitEffectTagList)
                                                {
                                                    if (pc.effectsList[j].tag == etfddl.tag)
                                                    {
                                                        if (pc.effectsList[j].isPermanent)
                                                        {
                                                            pc.effectsList.RemoveAt(j);
                                                        }
                                                    }
                                                }
                                            }
                                            //pc.replacedTraitsOrSpellsByTag.Add(tr.traitToReplaceByTag);
                                            pc.knownTraitsTags.RemoveAt(i);
                                        }
                                    }

                                    for (int i = pc.knownInCombatUsableTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownInCombatUsableTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            pc.knownInCombatUsableTraitsTags.RemoveAt(i);
                                        }

                                        if (pc.knownInCombatUsableTraitsTags[i] == temp2.associatedSpellTag)
                                        {
                                            pc.knownInCombatUsableTraitsTags.RemoveAt(i);
                                        }
                                    }

                                    for (int i = pc.knownOutsideCombatUsableTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownOutsideCombatUsableTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            pc.knownOutsideCombatUsableTraitsTags.RemoveAt(i);
                                        }
                                        if (pc.knownOutsideCombatUsableTraitsTags[i] == temp2.associatedSpellTag)
                                        {
                                            pc.knownOutsideCombatUsableTraitsTags.RemoveAt(i);
                                        }
                                    }

                                    for (int i = pc.knownUsableTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownUsableTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            pc.knownUsableTraitsTags.RemoveAt(i);
                                        }
                                        if (pc.knownUsableTraitsTags[i] == temp2.associatedSpellTag)
                                        {
                                            pc.knownUsableTraitsTags.RemoveAt(i);
                                        }
                                    }


                                    //adding trait to replace mechanism: learing traits list (just added)
                                    for (int i = pc.learningTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.learningTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            //TODO: remove connected permannent effects
                                            //Peter
                                            for (int j = pc.effectsList.Count - 1; j >= 0; j--)
                                            {
                                                foreach (EffectTagForDropDownList etfddl in temp2.traitEffectTagList)
                                                {
                                                    if (pc.effectsList[j].tag == etfddl.tag)
                                                    {
                                                        if (pc.effectsList[j].isPermanent)
                                                        {
                                                            pc.effectsList.RemoveAt(j);
                                                        }
                                                    }
                                                }
                                            }
                                            //pc.replacedTraitsOrSpellsByTag.Add(tr.traitToReplaceByTag);
                                            pc.learningTraitsTags.RemoveAt(i);
                                        }
                                    }
                                    #endregion

                                    //********************************************
                                    //add trait/effect system here: usable traits
                                    if (!tr.associatedSpellTag.Equals("none"))
                                    {
                                        if (tr.useableInSituation.Contains("Always"))
                                        {
                                            pc.knownUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownOutsideCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownInCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                        }
                                        if (tr.useableInSituation.Contains("OutOfCombat"))
                                        {
                                            pc.knownUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownOutsideCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                        }
                                        if (tr.useableInSituation.Contains("InCombat"))
                                        {
                                            pc.knownUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownInCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                        }
                                    }
                    
                    //add permanent effects of trait to effect list of this pc
                    foreach (EffectTagForDropDownList efTag in tr.traitEffectTagList)
                    {//1
                        foreach (Effect ef in gv.mod.moduleEffectsList)
                        {//2
                            if (ef.tag == efTag.tag)
                            {//3
                                if (ef.isPermanent)
                                {//4
                                    bool doesNotExistAlfready = true;
                                    foreach (Effect ef2 in pc.effectsList)
                                    {//5
                                        if (ef2.tag == ef.tag)
                                        {//6
                                            doesNotExistAlfready = false;
                                            break;
                                        }//6
                                    }//5

                                    if (doesNotExistAlfready)
                                    {//6
                                        pc.effectsList.Add(ef);
                                        gv.sf.UpdateStats(pc);
                                        if (ef.modifyHpMax != 0)
                                        {//7
                                            pc.hp += ef.modifyHpMax;
                                            if (pc.hp < 1)
                                            {//8
                                                pc.hp = 1;
                                            }//8
                                            if (pc.hp > pc.hpMax)
                                            {
                                                pc.hp = pc.hpMax;
                                            }
                                        }//7

                                        if (ef.modifyCon != 0)
                                        {//7
                                            pc.hp += ef.modifyCon / 2;
                                            if (pc.hp < 1)
                                            {//8
                                                pc.hp = 1;
                                            }//8
                                            if (pc.hp > pc.hpMax)
                                            {
                                                pc.hp = pc.hpMax;
                                            }
                                        }//7

                                        if (ef.modifySpMax != 0)
                                        {
                                            pc.sp += ef.modifySpMax;
                                            if (pc.sp < 1)
                                            {
                                                pc.sp = 1;
                                            }
                                            if (pc.sp > pc.spMax)
                                            {
                                                pc.sp = pc.spMax;
                                            }
                                        }

                                        if (ef.modifyStr != 0)
                                        {
                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("strength"))
                                            {
                                                pc.sp += ef.modifyStr / 2;
                                                if (pc.sp < 1)
                                                {
                                                    pc.sp = 1;
                                                }
                                                if (pc.sp > pc.spMax)
                                                {
                                                    pc.sp = pc.spMax;
                                                }
                                            }
                                        }

                                        if (ef.modifyDex != 0)
                                        {
                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("dexterity"))
                                            {
                                                pc.sp += ef.modifyDex / 2;
                                                if (pc.sp < 1)
                                                {
                                                    pc.sp = 1;
                                                }
                                                if (pc.sp > pc.spMax)
                                                {
                                                    pc.sp = pc.spMax;
                                                }
                                            }
                                        }

                                        if (ef.modifyCon != 0)
                                        {
                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("constitution"))
                                            {
                                                pc.sp += ef.modifyCon / 2;
                                                if (pc.sp < 1)
                                                {
                                                    pc.sp = 1;
                                                }
                                                if (pc.sp > pc.spMax)
                                                {
                                                    pc.sp = pc.spMax;
                                                }
                                            }
                                        }

                                        if (ef.modifyCha != 0)
                                        {
                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("charisma"))
                                            {
                                                pc.sp += ef.modifyCha / 2;
                                                if (pc.sp < 1)
                                                {
                                                    pc.sp = 1;
                                                }
                                                if (pc.sp > pc.spMax)
                                                {
                                                    pc.sp = pc.spMax;
                                                }
                                            }
                                        }

                                        if (ef.modifyInt != 0)
                                        {
                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("intelligence"))
                                            {
                                                pc.sp += ef.modifyInt / 2;
                                                if (pc.sp < 1)
                                                {
                                                    pc.sp = 1;
                                                }
                                                if (pc.sp > pc.spMax)
                                                {
                                                    pc.sp = pc.spMax;
                                                }
                                            }
                                        }

                                        if (ef.modifyWis != 0)
                                        {
                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("wisdom"))
                                            {
                                                pc.sp += ef.modifyWis / 2;
                                                if (pc.sp < 1)
                                                {
                                                    pc.sp = 1;
                                                }
                                                if (pc.sp > pc.spMax)
                                                {
                                                    pc.sp = pc.spMax;
                                                }
                                            }
                                        }
                                    }//5
                                }//4
                            }//3
                        }//2
                    }//1 

                                }
                                //note2
                                pc.learningTraitsTags.Clear();
                                pc.learningEffects.Clear();
                                pc.learningSpellsTags.Clear();
                                gv.screenPcCreation.SaveCharacter(pc);
                                gv.screenPartyBuild.pcList.Add(pc);
                                gv.screenType = "partyBuild";
                             }
                         }
                         else  
                         {
                            
                            //if there are spells to learn go to spell screen next  
                            List < string > spellTagsList = new List<string>();
                            spellTagsList = pc.getSpellsToLearn();
                            if (spellTagsList.Count > 0)
                            {
                                gv.screenSpellLevelUp.resetPC(false, pc, false);
                                gv.screenType = "learnSpellLevelUp";
                             }
                             else //no spells or traits to learn  
                             {

                                for (int counter = pc.learningTraitsTags.Count - 1; counter >= 0; counter--)
                                {
                                    pc.knownTraitsTags.Add(pc.learningTraitsTags[counter]);
                                    //TODO: must get trait by ts string
                                    foreach (Trait t in gv.mod.moduleTraitsList)
                                    {
                                        if (t.tag == pc.learningTraitsTags[counter])
                                        {
                                            tr = t;
                                            break;
                                        }
                                    }

                                    //******************************************************************
                                    #region replacement code traits
                                    //get the trait tor replace (if existent)
                                    Trait temp2 = new Trait();
                                    foreach (Trait t in gv.mod.moduleTraitsList)
                                    {
                                        if (t.tag == tr.traitToReplaceByTag)
                                        {
                                            temp2 = t.DeepCopy();
                                        }
                                    }

                                    if ((tr.traitToReplaceByTag != "none") && (tr.traitToReplaceByTag != ""))
                                    {
                                        pc.replacedTraitsOrSpellsByTag.Add(tr.traitToReplaceByTag);
                                    }

                                    if (tr.traitToReplaceByTag != tr.prerequisiteTrait)
                                    {
                                        string replacedTag = tr.traitToReplaceByTag;
                                        for (int j = gv.mod.moduleTraitsList.Count - 1; j >= 0; j--)
                                        {
                                            if (gv.mod.moduleTraitsList[j].prerequisiteTrait == replacedTag)
                                            {
                                                if (!pc.replacedTraitsOrSpellsByTag.Contains(replacedTag))
                                                {
                                                    pc.replacedTraitsOrSpellsByTag.Add(gv.mod.moduleTraitsList[j].tag);
                                                    replacedTag = gv.mod.moduleTraitsList[j].tag;
                                                    j = gv.mod.moduleTraitsList.Count - 1;
                                                }
                                            }
                                        }
                                    }

                                    //adding trait to replace mechanism: known traits
                                    for (int i = pc.knownTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            //TODO: remove connected permannent effects
                                            //Peter
                                            for (int j = pc.effectsList.Count - 1; j >= 0; j--)
                                            {
                                                foreach (EffectTagForDropDownList etfddl in temp2.traitEffectTagList)
                                                {
                                                    if (pc.effectsList[j].tag == etfddl.tag)
                                                    {
                                                        if (pc.effectsList[j].isPermanent)
                                                        {
                                                            pc.effectsList.RemoveAt(j);
                                                        }
                                                    }
                                                }
                                            }
                                            //pc.replacedTraitsOrSpellsByTag.Add(tr.traitToReplaceByTag);
                                            pc.knownTraitsTags.RemoveAt(i);
                                        }
                                    }

                                    for (int i = pc.knownInCombatUsableTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownInCombatUsableTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            pc.knownInCombatUsableTraitsTags.RemoveAt(i);
                                        }

                                        if (pc.knownInCombatUsableTraitsTags[i] == temp2.associatedSpellTag)
                                        {
                                            pc.knownInCombatUsableTraitsTags.RemoveAt(i);
                                        }
                                    }

                                    for (int i = pc.knownOutsideCombatUsableTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownOutsideCombatUsableTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            pc.knownOutsideCombatUsableTraitsTags.RemoveAt(i);
                                        }
                                        if (pc.knownOutsideCombatUsableTraitsTags[i] == temp2.associatedSpellTag)
                                        {
                                            pc.knownOutsideCombatUsableTraitsTags.RemoveAt(i);
                                        }
                                    }

                                    for (int i = pc.knownUsableTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.knownUsableTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            pc.knownUsableTraitsTags.RemoveAt(i);
                                        }
                                        if (pc.knownUsableTraitsTags[i] == temp2.associatedSpellTag)
                                        {
                                            pc.knownUsableTraitsTags.RemoveAt(i);
                                        }
                                    }


                                    //adding trait to replace mechanism: learing traits list (just added)
                                    for (int i = pc.learningTraitsTags.Count - 1; i >= 0; i--)
                                    {
                                        if (pc.learningTraitsTags[i] == tr.traitToReplaceByTag)
                                        {
                                            //TODO: remove connected permannent effects
                                            //Peter
                                            for (int j = pc.effectsList.Count - 1; j >= 0; j--)
                                            {
                                                foreach (EffectTagForDropDownList etfddl in temp2.traitEffectTagList)
                                                {
                                                    if (pc.effectsList[j].tag == etfddl.tag)
                                                    {
                                                        if (pc.effectsList[j].isPermanent)
                                                        {
                                                            pc.effectsList.RemoveAt(j);
                                                        }
                                                    }
                                                }
                                            }
                                            //pc.replacedTraitsOrSpellsByTag.Add(tr.traitToReplaceByTag);
                                            pc.learningTraitsTags.RemoveAt(i);
                                        }
                                    }
                                    #endregion

                                    //******************************************************************

                                    //add trait/effect system here: usable traits
                                    if (!tr.associatedSpellTag.Equals("none"))
                                    {
                                        if (tr.useableInSituation.Contains("Always"))
                                        {
                                            pc.knownUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownOutsideCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownInCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                        }
                                        if (tr.useableInSituation.Contains("OutOfCombat"))
                                        {
                                            pc.knownUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownOutsideCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                        }
                                        if (tr.useableInSituation.Contains("InCombat"))
                                        {
                                            pc.knownUsableTraitsTags.Add(tr.associatedSpellTag);
                                            pc.knownInCombatUsableTraitsTags.Add(tr.associatedSpellTag);
                                        }
                                    }

                                    //add permanent effects of trait to effect list of this pc
                                    foreach (EffectTagForDropDownList efTag in tr.traitEffectTagList)
                                    {//1
                                        foreach (Effect ef in gv.mod.moduleEffectsList)
                                        {//2
                                            if (ef.tag == efTag.tag)
                                            {//3
                                                if (ef.isPermanent)
                                                {//4
                                                    bool doesNotExistAlfready = true;
                                                    foreach (Effect ef2 in pc.effectsList)
                                                    {//5
                                                        if (ef2.tag == ef.tag)
                                                        {//6
                                                            doesNotExistAlfready = false;
                                                            break;
                                                        }//6
                                                    }//5

                                                    if (doesNotExistAlfready)
                                                    {//6
                                                        pc.effectsList.Add(ef);
                                                        gv.sf.UpdateStats(pc);
                                                        if (ef.modifyHpMax != 0)
                                                        {//7
                                                            pc.hp += ef.modifyHpMax;
                                                            if (pc.hp < 1)
                                                            {//8
                                                                pc.hp = 1;
                                                            }//8
                                                            if (pc.hp > pc.hpMax)
                                                            {
                                                                pc.hp = pc.hpMax;
                                                            }
                                                        }//7

                                                        if (ef.modifyCon != 0)
                                                        {//7
                                                            pc.hp += ef.modifyCon / 2;
                                                            if (pc.hp < 1)
                                                            {//8
                                                                pc.hp = 1;
                                                            }//8
                                                            if (pc.hp > pc.hpMax)
                                                            {
                                                                pc.hp = pc.hpMax;
                                                            }
                                                        }//7

                                                        if (ef.modifySpMax != 0)
                                                        {
                                                            pc.sp += ef.modifySpMax;
                                                            if (pc.sp < 1)
                                                            {
                                                                pc.sp = 1;
                                                            }
                                                            if (pc.sp > pc.spMax)
                                                            {
                                                                pc.sp = pc.spMax;
                                                            }
                                                        }

                                                        if (ef.modifyStr != 0)
                                                        {
                                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("strength"))
                                                            {
                                                                pc.sp += ef.modifyStr / 2;
                                                                if (pc.sp < 1)
                                                                {
                                                                    pc.sp = 1;
                                                                }
                                                                if (pc.sp > pc.spMax)
                                                                {
                                                                    pc.sp = pc.spMax;
                                                                }
                                                            }
                                                        }

                                                        if (ef.modifyDex != 0)
                                                        {
                                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("dexterity"))
                                                            {
                                                                pc.sp += ef.modifyDex / 2;
                                                                if (pc.sp < 1)
                                                                {
                                                                    pc.sp = 1;
                                                                }
                                                                if (pc.sp > pc.spMax)
                                                                {
                                                                    pc.sp = pc.spMax;
                                                                }
                                                            }
                                                        }

                                                        if (ef.modifyCon != 0)
                                                        {
                                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("constitution"))
                                                            {
                                                                pc.sp += ef.modifyCon / 2;
                                                                if (pc.sp < 1)
                                                                {
                                                                    pc.sp = 1;
                                                                }
                                                                if (pc.sp > pc.spMax)
                                                                {
                                                                    pc.sp = pc.spMax;
                                                                }
                                                            }
                                                        }

                                                        if (ef.modifyCha != 0)
                                                        {
                                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("charisma"))
                                                            {
                                                                pc.sp += ef.modifyCha / 2;
                                                                if (pc.sp < 1)
                                                                {
                                                                    pc.sp = 1;
                                                                }
                                                                if (pc.sp > pc.spMax)
                                                                {
                                                                    pc.sp = pc.spMax;
                                                                }
                                                            }
                                                        }

                                                        if (ef.modifyInt != 0)
                                                        {
                                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("intelligence"))
                                                            {
                                                                pc.sp += ef.modifyInt / 2;
                                                                if (pc.sp < 1)
                                                                {
                                                                    pc.sp = 1;
                                                                }
                                                                if (pc.sp > pc.spMax)
                                                                {
                                                                    pc.sp = pc.spMax;
                                                                }
                                                            }
                                                        }

                                                        if (ef.modifyWis != 0)
                                                        {
                                                            if (pc.playerClass.modifierFromSPRelevantAttribute.Equals("wisdom"))
                                                            {
                                                                pc.sp += ef.modifyWis / 2;
                                                                if (pc.sp < 1)
                                                                {
                                                                    pc.sp = 1;
                                                                }
                                                                if (pc.sp > pc.spMax)
                                                                {
                                                                    pc.sp = pc.spMax;
                                                                }
                                                            }
                                                        }
                                                    }//5
                                                }//4
                                            }//3
                                        }//2
                                    }//1 

                                }
                                pc.learningTraitsTags.Clear();
                                pc.learningEffects.Clear();
                                pc.learningSpellsTags.Clear();
                                pc.classLevel--;
                                pc.LevelUp();
                                gv.sf.UpdateStats(pc);
                                gv.screenType = "party";
                                gv.screenParty.traitGained += tr.name + ", ";
                                gv.screenParty.doLevelUpSummary();
                             }
                        }
                  }
                }
                else
			    {
				    gv.sf.MessageBox("Can't learn that trait, try another or exit");
			    }
		    }   	
        }
            
        public bool isAvailableToLearn(string spellTag)
        {
    	    if (traitsToLearnTagsList.Contains(spellTag))
    	    {
    		    return true;
    	    }
    	    return false;
        }    
        public void fillToLearnList()
        {
    	    traitsToLearnTagsList = pc.getTraitsToLearn(gv.mod);	    
        }    
        public Trait GetCurrentlySelectedTrait()
	    {
            sortTraitsForLevelUp(pc);
            //TraitAllowed ta = pc.playerClass.traitsAllowed[traitSlotIndex + (tknPageIndex * slotsPerPage)];
            TraitAllowed ta = backupTraitsAllowed[traitSlotIndex + (tknPageIndex * slotsPerPage)];
            return gv.mod.getTraitByTag(ta.tag);
	    }
	    public bool isSelectedTraitSlotInKnownTraitsRange()
	    {
            //return (traitSlotIndex + (tknPageIndex * slotsPerPage)) < pc.playerClass.traitsAllowed.Count;
            sortTraitsForLevelUp(pc);
            return (traitSlotIndex + (tknPageIndex * slotsPerPage)) < backupTraitsAllowed.Count;
        }	
	    public int getLevelAvailable(string tag)
	    {
		    TraitAllowed ta = pc.playerClass.getTraitAllowedByTag(tag);
		    if (ta != null)
		    {
			    return ta.atWhatLevelIsAvailable;
		    }
		    return 0;
	    }
	    public void tutorialMessageTraitScreen()
        {
		    gv.sf.MessageBoxHtml(this.stringMessageTraitLevelUp);	
        }
    }
}
