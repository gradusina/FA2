using System.Windows.Media;

namespace FA2.Converters
{
    internal class BooleanToBrushConverter : BooleanConverter<Brush>
    {
        public BooleanToBrushConverter() : base(Brushes.LightGray, Brushes.White) { }
    }
}
