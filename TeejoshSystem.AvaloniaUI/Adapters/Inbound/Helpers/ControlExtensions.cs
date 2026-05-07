using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Helpers
{
    public static class ControlExtensions
    {
        // Propiedad para bloquear scroll en ComboBox
        public static readonly AttachedProperty<bool> BlockScrollProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("BlockScroll", typeof(ControlExtensions));

        // Propiedad para rastrear si el usuario ha interactuado con el control
        public static readonly AttachedProperty<bool> IsTouchedProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("IsTouched", typeof(ControlExtensions), false);

        static ControlExtensions()
        {
            // Lógica de bloqueo de scroll para ComboBox
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

            // Lógica para marcar como "tocado" al recibir el foco
            Control.GotFocusEvent.AddClassHandler<Control>((x, e) =>
            {
                if (x is TextBox || x is NumericUpDown || x is ComboBox)
                {
                    x.SetValue(IsTouchedProperty, true);
                }
            }, RoutingStrategies.Bubble);
        }

        public static void SetBlockScroll(Control element, bool value) => element.SetValue(BlockScrollProperty, value);
        public static bool GetBlockScroll(Control element) => element.GetValue(BlockScrollProperty);

        public static void SetIsTouched(Control element, bool value) => element.SetValue(IsTouchedProperty, value);
        public static bool GetIsTouched(Control element) => element.GetValue(IsTouchedProperty);
    }
}
