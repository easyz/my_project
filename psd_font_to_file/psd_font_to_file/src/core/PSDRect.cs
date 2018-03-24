﻿namespace PsdParser
{
    public struct PSDRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public int width
        {
            get
            {
                return this.right - this.left;
            }
        }

        public int height
        {
            get
            {
                return this.bottom - this.top;
            }
        }
    }
}
