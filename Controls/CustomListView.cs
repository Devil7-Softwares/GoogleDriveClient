using System;
using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI.Controls
{
    public class CustomListView : ListView
    {
        #region Constructor
        public CustomListView(IListDataSource source) : base(source)
        {
        }
        #endregion

        #region Custom Events
        public delegate void KeyPressedEventHandler(object sender, KeyPressedEventArgs e);
        public event KeyPressedEventHandler OnKeyPressed;
        #endregion

        #region Overrides
        public override bool ProcessKey(KeyEvent kb)
        {
            this.OnKeyPressed?.Invoke(this, new KeyPressedEventArgs(kb));
            return base.ProcessKey(kb);
        }
        #endregion
    }

    public class KeyPressedEventArgs : EventArgs
    {
        #region Properties
        public KeyEvent KeyEvent { get; }
        #endregion

        #region Constructor
        public KeyPressedEventArgs(KeyEvent keyEvent)
        {
            this.KeyEvent = keyEvent;
        } 
        #endregion
    }
}
