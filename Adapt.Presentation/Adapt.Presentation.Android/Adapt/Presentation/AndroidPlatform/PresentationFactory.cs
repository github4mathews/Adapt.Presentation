using Android.Content;
using System;

namespace Adapt.Presentation.AndroidPlatform
{
    public class PresentationFactory : IPresentationFactory
    {
        public Context Context { get; private set; }

        public PresentationFactory(Context context)
        {
            Context = context;
        }

        public IFilePicker CreateFilePicker()
        {
            return new FilePicker(Context);
        }

        public IMedia CreateMedia()
        {
            throw new NotImplementedException();
        }
    }
}