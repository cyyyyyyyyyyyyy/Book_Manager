using System;

namespace fileManager2022
{
    [Serializable] internal class Environment
    {
        internal string currFont = "Microsoft Sans Serif";
        internal System.Drawing.Color currBgCol;
        internal string[] fontCollection;

        internal Environment()
        {
            fontCollection = new string[] { "Mongolian Baiti", "Times New Roman", "SimSun-ExtB", "Microsoft Sans Serif", "Cascadia Mono",
            "Courier New", "Gabriola", "Lucida Sans Unicode", "Sitka Display"};
            currBgCol = System.Drawing.Color.White;
        }
    }
}
