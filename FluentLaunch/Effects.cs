using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Markup;

namespace FluentLaunch
{
    #region Window Blur

    /// <summary>
    /// 为窗口提供模糊特效。
    /// </summary>
    public class WindowAccentCompositor
    {
        private readonly Window _window;
        private bool _isEnabled;
        private int _blurColor;

        /// <summary>
        /// 创建 <see cref="WindowAccentCompositor"/> 的一个新实例。
        /// </summary>
        /// <param name="window">要创建模糊特效的窗口实例。</param>
        public WindowAccentCompositor(Window window) => _window = window ?? throw new ArgumentNullException(nameof(window));

        /// <summary>
        /// 获取或设置此窗口模糊特效是否生效的一个状态。
        /// 默认为 false，即不生效。
        /// </summary>
        [DefaultValue(false)]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnIsEnabledChanged(value);
            }
        }

        /// <summary>
        /// 获取或设置此窗口模糊特效叠加的颜色。
        /// </summary>
        public Color Color
        {
            get => Color.FromArgb(
                // 取出红色分量。
                (byte)((_blurColor & 0x000000ff) >> 0),
                // 取出绿色分量。
                (byte)((_blurColor & 0x0000ff00) >> 8),
                // 取出蓝色分量。
                (byte)((_blurColor & 0x00ff0000) >> 16),
                // 取出透明分量。
                (byte)((_blurColor & 0xff000000) >> 24));
            set => _blurColor =
                // 组装红色分量。
                value.R << 0 |
                // 组装绿色分量。
                value.G << 8 |
                // 组装蓝色分量。
                value.B << 16 |
                // 组装透明分量。
                value.A << 24;
        }

        private void OnIsEnabledChanged(bool isEnabled)
        {
            Window window = _window;
            var handle = new WindowInteropHelper(window).EnsureHandle();
            Composite(handle, isEnabled);
        }

        private void Composite(IntPtr handle, bool isEnabled)
        {
            // 创建 AccentPolicy 对象。
            var accent = new AccentPolicy();

            // 设置特效。
            if (!isEnabled)
            {
                accent.AccentState = AccentState.ACCENT_DISABLED;
            }
            else
            {
                // 亚克力效果
                accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                accent.GradientColor = _blurColor;

                // Windows 10 早期的模糊特效
                // accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            }

            // 将托管结构转换为非托管对象。
            var accentPolicySize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentPolicySize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            // 设置窗口组合特性。
            try
            {
                // 设置模糊特效。
                var data = new WindowCompositionAttributeData
                {
                    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    SizeOfData = accentPolicySize,
                    Data = accentPtr,
                };
                SetWindowCompositionAttribute(handle, ref data);
            }
            finally
            {
                // 释放非托管对象。
                Marshal.FreeHGlobal(accentPtr);
            }
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private enum AccentState
        {
            /// <summary>
            /// 完全禁用 DWM 的叠加特效。
            /// </summary>
            ACCENT_DISABLED = 0,

            /// <summary>
            ///
            /// </summary>
            ACCENT_ENABLE_GRADIENT = 1,

            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            // 省略其他未使用的字段
            WCA_ACCENT_POLICY = 19,

            // 省略其他未使用的字段
        }

        /// <summary>
        /// 当前操作系统支持的透明模糊特效级别。
        /// </summary>
        public enum BlurSupportedLevel
        {
            /// <summary>
            ///
            /// </summary>
            NotSupported,

            Aero,
            Blur,
            Acrylic,
        }
    }

    #endregion Window Blur

    #region Reveal Effect

    /// <summary>
    /// Paints a control border with a reveal effect using composition brush and light effects.
    /// </summary>
    public class RevealBorderBrushExtension : MarkupExtension
    {
        [ThreadStatic]
        private static Dictionary<RadialGradientBrush, WeakReference<FrameworkElement>> GlobalRevealingElements;

        /// <summary>
        /// The color to use for rendering in case the <see cref="MarkupExtension"/> can't work correctly.
        /// </summary>
        public Color FallbackColor { get; set; } = Colors.White;

        /// <summary>
        /// Gets or sets a value that specifies the base background color for the brush.
        /// </summary>
        public Color Color { get; set; } = Colors.White;

        public Transform Transform { get; set; } = Transform.Identity;

        public Transform RelativeTransform { get; set; } = Transform.Identity;

        public double Opacity { get; set; } = 1.0;

        public double Radius { get; set; } = 100.0;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 如果没有服务，则直接返回。
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
            {
                return null;
            }

            // MarkupExtension 在样式模板中，返回 this 以延迟提供值。
            if (service.TargetObject.GetType().Name.EndsWith("SharedDp", StringComparison.Ordinal))
            {
                return this;
            }

            if (!(service.TargetObject is FrameworkElement element))
            {
                return this;
            }

            if (DesignerProperties.GetIsInDesignMode(element))
            {
                return new SolidColorBrush(FallbackColor);
            }

            var brush = CreateGlobalBrush(element);
            return brush;
        }

        private Brush CreateBrush(UIElement rootVisual, FrameworkElement element)
        {
            var brush = CreateRadialGradientBrush();
            rootVisual.MouseMove += OnMouseMove;
            return brush;

            void OnMouseMove(object sender, MouseEventArgs e) => UpdateBrush(brush, e.GetPosition(element));
        }

        private Brush CreateGlobalBrush(FrameworkElement element)
        {
            var brush = CreateRadialGradientBrush();
            if (GlobalRevealingElements is null)
            {
                CompositionTarget.Rendering -= OnRendering;
                CompositionTarget.Rendering += OnRendering;
                GlobalRevealingElements = new Dictionary<RadialGradientBrush, WeakReference<FrameworkElement>>();
            }

            GlobalRevealingElements.Add(brush, new WeakReference<FrameworkElement>(element));
            return brush;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (GlobalRevealingElements is null)
            {
                return;
            }

            var toCollect = new List<RadialGradientBrush>();
            foreach (var pair in GlobalRevealingElements)
            {
                var brush = pair.Key;
                var weak = pair.Value;
                if (weak.TryGetTarget(out var element))
                {
                    Reveal(brush, element);
                }
                else
                {
                    toCollect.Add(brush);
                }
            }

            foreach (var brush in toCollect)
            {
                GlobalRevealingElements.Remove(brush);
            }

            void Reveal(RadialGradientBrush brush, IInputElement element) => UpdateBrush(brush, Mouse.GetPosition(element));
        }

        private void UpdateBrush(RadialGradientBrush brush, Point origin)
        {
            if (IsUsingCursorPointerDevice())
            {
                brush.GradientOrigin = origin;
                brush.Center = origin;
            }
            else
            {
                brush.Center = new Point(double.NegativeInfinity, double.NegativeInfinity);
            }
        }

        private RadialGradientBrush CreateRadialGradientBrush()
        {
            var brush = new RadialGradientBrush(Color, Colors.Transparent)
            {
                MappingMode = BrushMappingMode.Absolute,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = Opacity,
                Transform = Transform,
                RelativeTransform = RelativeTransform,
                Center = new Point(double.NegativeInfinity, double.NegativeInfinity),
            };
            return brush;
        }

        /// <summary>
        /// 判断当前正在使用的点输入设备是否是包含指示位置的指针型输入设备。
        /// 在笔、触摸和触笔三种输入中，笔和触笔会在实际操作之前显示表示其位置的光标，而触摸则只能在发生操作时得知其位置；
        /// 因此，这三种输入方式中，只有笔和触笔会返回 true，其他返回 false。
        /// </summary>
        private bool IsUsingCursorPointerDevice()
        {
            var device = Stylus.CurrentStylusDevice;
            if (device is null)
            {
                return true;
            }

            if (device.TabletDevice.Type == TabletDeviceType.Stylus)
            {
                return true;
            }

            return false;
        }
    }

    #endregion Reveal Effect
}