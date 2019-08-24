using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ThunderHawk
{
    public class IndicatorEffect : ShaderEffect
    {
        private static PixelShader pixelShader = new PixelShader();

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(IndicatorEffect), 0);

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(float), typeof(IndicatorEffect), new UIPropertyMetadata(0f, PixelShaderConstantCallback(0)));

        static IndicatorEffect()
        {
            pixelShader.SetStreamSource(File.OpenRead("Effects/IndicatorEffect.ps"));
        }

        public IndicatorEffect()
        {
            PixelShader = pixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(TimeProperty);
            CompositionTarget.Rendering += OnRender;
        }

        ~IndicatorEffect()
        {
            CompositionTarget.Rendering -= OnRender;
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        void OnRender(object sender, EventArgs e)
        {
            SetValue(TimeProperty, ((float)Environment.TickCount) / 1000f);
        }
    }
}
