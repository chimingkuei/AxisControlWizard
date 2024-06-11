using DeepWise.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace DeepWise.Controls
{
    public partial class Display
    {
        public void Save(string path)
        {
            if (Image != null)
            {
#if MAT
                Image.SaveImage(path);
#else
                Image.Save(path);
#endif
            }
            else if (Camera != null)
            {
                (Camera as ICamera).Capture().SaveImage(path);
         
            }
        }
        public void Stretch()
        {
            if (ImageWidth == 0 || ImageHeight == 0) return;
            if (ActualWidth == 0 || ActualHeight == 0) return;
            var wr = (float)ActualWidth / ImageWidth;
            var hr = (float)ActualHeight / ImageHeight;
            var zoomLevel = wr < hr ? wr : hr;
            var nw = (int)(ImageWidth * zoomLevel);
            var nh = (int)(ImageHeight * zoomLevel);
            var offsetX = (ActualWidth - nw) / 2;
            var offsetY = (ActualHeight - nh) / 2;
            _transform.Matrix = new Matrix(zoomLevel, 0, 0, zoomLevel, offsetX, offsetY);
            ZoomLevel = _transform.Matrix.M11;
            UpdateStrokeThickness();
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => Save(),
                        param => Image != null || Camera != null
                    );
                }
                return _saveCommand;
                void Save()
                {
                    using (var sfd = new System.Windows.Forms.SaveFileDialog())
                    {
                        sfd.Filter = "Images|*.bmp";
                        ImageFormat format = ImageFormat.Png;
                        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string ext = System.IO.Path.GetExtension(sfd.FileName);
                            switch (ext)
                            {
                                case ".jpg":
                                    format = ImageFormat.Jpeg;
                                    break;
                                case ".bmp":
                                    format = ImageFormat.Bmp;
                                    break;
                            }
                            this.Save(sfd.FileName);
                        }
                    }
                }
            }
        }
        public ICommand StretchCommand
        {
            get
            {
                if (_stretchCommand == null)
                {
                    _stretchCommand = new RelayCommand(
                        param => this.Stretch(),
                        param => true
                    );
                }
                return _stretchCommand;
            }
        }

#region
        private ICommand _saveCommand;
        private ICommand _stretchCommand;
#endregion
    }

    public class RelayCommand : ICommand
    {
#region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

#endregion // Fields

#region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

#endregion // Constructors

#region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return _canExecute == null ? true : _canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            _execute(parameters);
        }

#endregion // ICommand Members
    }
}
