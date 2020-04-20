using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FA2.Behaviors
{
    public class TextBlockAdorner : Adorner
    {
        private readonly TextBlock _mTextBlock;

        public TextBlockAdorner(UIElement adornedElement, string label, Style labelStyle)
            : base(adornedElement)
        {
            _mTextBlock = new TextBlock {Style = labelStyle, Text = label};
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _mTextBlock.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _mTextBlock.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _mTextBlock;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
    }
}