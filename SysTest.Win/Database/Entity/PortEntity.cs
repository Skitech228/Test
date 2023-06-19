#region Using derectives

using GalaSoft.MvvmLight;

#endregion

namespace SysTest.Win.Database.Entity
{
    public class PortEntity : ViewModelBase
    {
        public PortEntity(Port port) => Entity = port;

        #region _portEntity Property

        /// <summary>
        ///     Private member backing variable for <see cref="MyProperty" />
        /// </summary>
        private Port _portEntity;

        /// <summary>
        ///     Gets and sets The property's value
        /// </summary>
        public Port Entity
        {
            get => _portEntity;
            set { Set(() => Entity, ref _portEntity, value); }
        }

        #endregion
    }
}