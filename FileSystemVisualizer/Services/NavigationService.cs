using System;
using System.Windows.Controls;

namespace FileSystemVisualizer.Services
{
    public class NavigationService
    {
        private Frame? _frame;

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(Page page)
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame no ha sido configurado.");
            
            _frame.Navigate(page);
        }

        public void GoBack()
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame no ha sido configurado.");
            
            if (_frame.CanGoBack)
                _frame.GoBack();
        }
    }
}
