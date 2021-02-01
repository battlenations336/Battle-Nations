using System;

namespace BNR
{
    public class ButtonEventArgs : EventArgs
    {
        public ButtonValue ButtonValue;
        public string StringValue;

        public ButtonEventArgs(ButtonValue ButtonValue, string StringValue)
        {
            this.ButtonValue = ButtonValue;
            this.StringValue = StringValue;
        }
    }
}
