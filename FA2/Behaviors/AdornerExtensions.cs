using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FA2.Behaviors
{
    public static class AdornerExtensions
    {
        public static void TryRemoveAdorners<T>(this UIElement elem)
            where  T : Adorner
        {
            var adornerLayer =  AdornerLayer.GetAdornerLayer(elem);
            if (adornerLayer != null)
            {
                adornerLayer.RemoveAdorners<T>(elem);
            }
        }

        public static void RemoveAdorners<T>(this AdornerLayer adr, UIElement elem)
            where T : Adorner
        {
            var adorners = adr.GetAdorners(elem);

            if (adorners == null) return;

            for (var i = adorners.Length - 1; i >= 0; i--)
            {
                if (adorners[i] is T)
                    adr.Remove(adorners[i]);
            }
        }

        public static void TryAddAdorner<T>(this UIElement elem, Adorner adorner)
            where T : Adorner
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(elem);
            if (adornerLayer != null && !adornerLayer.ContainsAdorner<T>(elem))
            {
                Panel.SetZIndex(adorner, 0);
                adornerLayer.Add(adorner);
                
            }
        }

        public static bool ContainsAdorner<T>(this AdornerLayer adr, UIElement elem)
            where T : Adorner
        {
            var adorners = adr.GetAdorners(elem);

            if (adorners == null) return false;

            for (var i = adorners.Length - 1; i >= 0; i--)
            {
                if (adorners[i] is T)
                    return true;
            }
            return false;
        }

        public static void RemoveAllAdorners(this AdornerLayer adr, UIElement elem)
        {
            var adorners = adr.GetAdorners(elem);

            if (adorners == null) return;

            foreach (var toRemove in adorners)
                adr.Remove(toRemove);
        }
    }
}