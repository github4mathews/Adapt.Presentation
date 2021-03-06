﻿namespace Adapt.Presentation.UWP
{
    public class PresentationFactory : IPresentationFactory
    {
        #region Implementation
        public IFilePicker CreateFilePicker()
        {
            return new FilePicker();
        }

        public IMedia CreateMedia(IPermissions currentPermissions)
        {
            return new Media();
        }
        #endregion
    }
}