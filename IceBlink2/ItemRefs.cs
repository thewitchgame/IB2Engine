﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IceBlink2
{
    public class ItemRefs 
    {
	    public string resref = "none";
	    public string tag = "none";
	    public string name = "none";
	    public bool canNotBeUnequipped = false;
	    public int quantity = 1; //useful for stacking and ammo
        public bool isRation = false;
        public bool isLightSource = false;
        public int hpRegenTimer = 0;
        public int spRegenTimer = 0;
    
        public ItemRefs()
        {
    	
        }
        public ItemRefs DeepCopy()
	    {
		    ItemRefs copy = new ItemRefs();
            copy.isRation = this.isRation;
            copy.isLightSource = this.isLightSource;
            copy.tag = this.tag;
		    copy.name = this.name;
		    copy.resref = this.resref;
		    copy.canNotBeUnequipped = this.canNotBeUnequipped; 	
		    copy.quantity = this.quantity;
            copy.hpRegenTimer = this.hpRegenTimer;
            copy.spRegenTimer = this.spRegenTimer;
		    return copy;
	    }    
    }
}
