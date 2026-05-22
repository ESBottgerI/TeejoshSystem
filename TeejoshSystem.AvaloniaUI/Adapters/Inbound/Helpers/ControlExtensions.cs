using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Helpers
{
    public static class ControlExtensions
    {
        public static readonly AttachedProperty<bool> BlockScrollProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("BlockScroll", typeof(ControlExtensions));

        static ControlExtensions()
        {
            BlockScrollProperty.Changed.AddClassHandler<ComboBox>((x, e) =>
            {
                if (e.NewValue is bool block && block)
                {
                    x.AddHandler(Control.PointerWheelChangedEvent, (s, ev) =>
                    {
                        if (!x.IsDropDownOpen)
                        {
                            ev.Handled = true;
                        }
                    }, RoutingStrategies.Tunnel);
                }
            });
        }

        public static void SetBlockScroll(Control element, bool value) => element.SetValue(BlockScrollProperty, value);
        public static bool GetBlockScroll(Control element) => element.GetValue(BlockScrollProperty);
    }
}