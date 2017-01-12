namespace MELHARFI
{
    public static class ZOrder
    {
        static int bgr;
        static int obj;
        static int ctrl;
        static int top;

        /// <summary>
        /// Increment Zindex by 1 of the Bgr layer
        /// </summary>
        /// <returns>Return an int value</returns>
        public static int Bgr()
        {
            bgr++;
            return bgr;
        }

        /// <summary>
        /// Increment Zindex by 1 of the Obj layer
        /// </summary>
        /// <returns>Return an int value</returns>
        public static int Obj()
        {
            obj++;
            return obj;
        }

        /// <summary>
        /// Increment Zindex by 1 of the Ctrl layer
        /// </summary>
        /// <returns>Return an int value</returns>
        public static int Ctrl()
        {
            ctrl++;
            return ctrl;
        }

        /// <summary>
        /// Increment Zindex by 1 of the Top layer
        /// </summary>
        /// <returns>Return an int value</returns>
        public static int Top()
        {
            top++;
            return top;
        }

        /// <summary>
        /// Make all index of all layer equal to 0
        /// </summary>
        public static void Clear()
        {
            bgr = 0;
            obj = 0;
            ctrl = 0;
            top = 0;
        }

        /// <summary>
        /// Make the index of the selected layer equal to 0
        /// </summary>
        /// <param name="listObj">String value if equal to "bgr" then the Bgr layer is reseted, if equal to "obj" then Obj layer is reseted, if equal to ctrl then "Ctrl" layer is reseted, if equal to "top" then Top layer is reseted</param>
        public static void Clear(string listObj)
        {
            switch (listObj)
            {
                case "bgr":
                    bgr = 0;
                    break;
                case "obj":
                    obj = 0;
                    break;
                case "ctrl":
                    ctrl = 0;
                    break;
                case "top":
                    top = 0;
                    break;
            }
        }
    }
}
