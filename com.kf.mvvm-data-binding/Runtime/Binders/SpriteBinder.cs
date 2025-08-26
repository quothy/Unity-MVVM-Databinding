using UnityEngine;
using UnityEngine.UI;
using MVVMDatabinding;

namespace MVVMDatabinding
{
    // Binds a Sprite property in the ViewModel to the sprite of a UnityEngine.UI.Image
    public class SpriteBinder : BaseBinder<Sprite>
    {
        [SerializeField]
        private Image targetImage = null;

        [SerializeField]
        private bool disableIfNull = true;

        protected override void OnDataUpdated(Sprite value)
        {
            if (targetImage != null)
            {
                targetImage.sprite = value;

                if (value == null && disableIfNull)
                {
                    targetImage.enabled = false;
                }
                else if (value != null && !targetImage.enabled)
                {
                    targetImage.enabled = true;
                }
            }
        }
    }
}
