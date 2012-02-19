#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using System.IO;

#endregion

namespace Simulator.Utils
{
    public static class ParseHelper
    {
        public static string GetResourceText(string filepath)
        {
            FileStream stream = File.OpenRead(filepath);

            TextReader textReader = new StreamReader(stream);
            string result = textReader.ReadToEnd();
            textReader.Close();

            return result;
        }
    }
}