﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NS_Education.Models
{

    public class PublicModel
    {
        
    }
    public class cSelectItem
    {
        public int ID = 0;
        public string Title = "";
        public bool SelectFlag = false;
    }
    public class cReturnMessage
    {
        public bool Success = true;
        public string Message = "";
    }
}