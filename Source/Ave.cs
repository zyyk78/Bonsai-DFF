using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

[Combinator]
[Description("")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class Ave
{
    private const int Cap=128;
    public IObservable<IplImage> Process(IObservable<IplImage> source)
    {
        return Observable.Create<IplImage>(observer => 
        {
            var imageQueue = new ConcurrentQueue<IplImage>();
            return source.Subscribe( image =>
            {
                if(imageQueue.Count >= Cap)
                {
                    IplImage OP;
                    imageQueue.TryDequeue(out OP);
                }
                imageQueue.Enqueue(image);
                if(imageQueue.Count>0) { IplImage Out = CalculateAverageImage(imageQueue); observer.OnNext(Out);}
                else { observer.OnNext(image);}
            },
            observer.OnError,
            observer.OnCompleted);
        });
    }

    private IplImage CalculateAverageImage(ConcurrentQueue<IplImage> imageQueue)
    {
        if (!imageQueue.Any())
        {
            throw new InvalidOperationException("The image queue is empty.");
        }

        // Assume images are the same size and type, e.g., CV_8UC3

        // Initialize an accumulator for sum
        IplImage accumulator = new IplImage(imageQueue.First().Size,IplDepth.F32,imageQueue.First().Channels);
        IplImage result = new IplImage(imageQueue.First().Size,imageQueue.First().Depth,imageQueue.First().Channels);
        foreach (var image in imageQueue)
        {
            IplImage image1 = image;
            CV.Add(accumulator, image1, accumulator);
        }
        accumulator=accumulator/imageQueue.Count;
        CV.ConvertScaleAbs(accumulator,result);
        return result;
    }
}
