using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

[Combinator]
[Description("")]
[WorkflowElementCategory(ElementCategory.Combinator)]
public class Div
{
    [Description("Scale")]
    public float scale { get; set; }

    [Description("Shift")]
    public float shift { get; set; }

    public IObservable<IplImage> Process(IObservable<Tuple<IplImage,IplImage>> source)
    {
        return Observable.Create<IplImage>(observer => 
        {

            return source.Subscribe( images =>
            {
                IplImage RawP=images.Item1;
                IplImage AveP=images.Item2;
                IplImage result = new IplImage(RawP.Size,IplDepth.F32,RawP.Channels);
                IplImage output = new IplImage(RawP.Size,RawP.Depth,RawP.Channels);
                IplImage Ref = new IplImage(RawP.Size,RawP.Depth,RawP.Channels);
                CV.Div(AveP,RawP,result);
                CV.ConvertScaleAbs(result,output,scale,shift);

                CV.ConvertScaleAbs(Ref,Ref,0,255);
                CV.Min(output,Ref,output);
                CV.ConvertScaleAbs(Ref,Ref,0,0);
                CV.Max(output,Ref,output);

                observer.OnNext(output);

            },
            observer.OnError,
            observer.OnCompleted);
        });
    }
}
