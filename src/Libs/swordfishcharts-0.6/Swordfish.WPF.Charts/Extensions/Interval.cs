// Copyright © 2007 by Initial Force AS.  All rights reserved.
//
// Author: Andreas Larsen
 

using System;

namespace Swordfish.WPF.Charts.Extensions
{
    public class Interval
    {
        private readonly double _intervalEnd;
        private readonly double _intervalStart;
        private string _name;

        public Interval(String name, double intervalStart, double intervalEnd)
        {
            _name = name;
            _intervalStart = intervalStart;
            _intervalEnd = intervalEnd; 
        }

        public double End
        {
            get { return _intervalEnd; }
        }

        public double Start
        {
            get { return _intervalStart; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}