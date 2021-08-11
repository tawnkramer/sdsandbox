using UnityEngine.UI;

namespace UIHelper
{
    /// <summary>
    /// A wrapper for UI slider with text so as to simplify the process of modifying both elements
    /// </summary>
    [System.Serializable]
    public struct SliderWithText
    {
        public Slider slider;
        private Text _text;

        public float Value
        {
            get { return slider.value; }
            set
            { slider.value = value; }
        }

        public string Text
        {
            get
            {
                if (!_text)
                    _text = slider.GetComponentInChildren<Text>();
                return _text.text;
            }
            set
            {
                if (!_text)
                    _text = slider.GetComponentInChildren<Text>();
                _text.text = value;
            }
        }
    }
}
