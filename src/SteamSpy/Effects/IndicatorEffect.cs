using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ThunderHawk.Effects
{
    public class IndicatorEffect : ShaderEffect
    {
        private static PixelShader pixelShader = new PixelShader();
        private static PropertyInfo propertyInfo;

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(float), typeof(IndicatorEffect), new UIPropertyMetadata(0f, PixelShaderConstantCallback(0)));

        static IndicatorEffect()
        {
            pixelShader.SetStreamSource(File.OpenRead("Effects/IndicatorEffect.ps"));
            propertyInfo = typeof(RectBlurEffect).GetProperty("InheritanceContext",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public IndicatorEffect()
        {
            CompositionTarget.Rendering += OnRender;
        }

        ~IndicatorEffect()
        {
            CompositionTarget.Rendering -= OnRender;
        }

        void OnRender(object sender, EventArgs e)
        {
            SetValue(TimeProperty, ((float)Environment.TickCount) / 1000f);
        }
    }
}
