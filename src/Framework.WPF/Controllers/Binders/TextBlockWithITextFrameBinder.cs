using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Framework.WPF
{
    public class TextBlockWithITextFrameBinder : BindingController<TextBlock, ITextFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(ITextFrame.Text), OnTextChanged);
            OnTextChanged();
        }

        void OnTextChanged()
        {
            View.Inlines.Clear();

            var text = Frame.Text;

            if (text.IsNullOrWhiteSpace())
                return;

            var builder = new StringBuilder();

            string tag = null;
            string previousValue = null;



            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (ch == '<')
                {
                    previousValue = builder.ToString();
                    builder.Clear();
                    continue;
                }

                if (ch == '>')
                {
                    var thisTag = builder.ToString();

                    if (thisTag.Length == 0)
                        continue;

                    if (thisTag[0] == '/')
                    {
                        if (thisTag.Substring(1) == tag)
                        {
                            switch (tag)
                            {
                                case "db":
                                    View.Inlines.Add(new Run(previousValue) { FontWeight = FontWeights.DemiBold });
                                    break;
                                case "m":
                                    View.Inlines.Add(new Run(previousValue) { FontWeight = FontWeights.Medium });
                                    break;
                                case "l":
                                    View.Inlines.Add(new Run(previousValue) { FontWeight = FontWeights.Light });
                                    break;
                                case "sb":
                                    View.Inlines.Add(new Run(previousValue) { FontWeight = FontWeights.SemiBold });
                                    break;
                                case "u":
                                    View.Inlines.Add(new Underline(new Run(previousValue)));
                                    break;
                                case "i":
                                    View.Inlines.Add(new Italic(new Run(previousValue)));
                                    break;
                                case "b":
                                    View.Inlines.Add(new Bold(new Run(previousValue)));
                                    break;
                                case "href":
                                    {
                                        var link = new Hyperlink(new Run(previousValue));
                                        link.Click += (s,e) => OnNavigate(new Uri(previousValue));
                                        View.Inlines.Add(link);
                                    }
                                    break;
                                default:
                                    View.Inlines.Add(new Run(previousValue));
                                    break;
                            }
                        }

                        builder.Clear();
                        continue;
                    }
                    else
                    {
                        View.Inlines.Add(previousValue);
                        builder.Clear();
                        tag = thisTag;
                    }

                    continue;
                }

                builder.Append(ch);
            }

            View.Inlines.Add(new Run(builder.ToString()));
        }

        void OnNavigate(Uri uri)
        {
            (Frame as IActionFrame)?.ActionWithParameter?.Invoke(uri);
        }
    }
}
