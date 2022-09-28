using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;

namespace Launcher
{
    public class Shortcut
    {
        public string Name { get; set; }    
        public string Path{ get; set; }

        private Icon _icon;
        public BitmapSource Icon
        {
            get
            {
                if (_icon == null)
                {
                    try
                    {
                        _icon = System.Drawing.Icon.ExtractAssociatedIcon(Path);
                     
                    }
                    catch
                    {
                        return null;
                    }
                }
                return Imaging.CreateBitmapSourceFromHIcon(_icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            
        }
    }
}
