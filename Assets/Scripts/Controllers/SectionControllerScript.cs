using TMPro;
using UnityEngine;

namespace IslandConfig.Controllers
{
    internal class SectionControllerScript : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sectionLabel;
        [SerializeField] public RectTransform itemsContainer;

        public void Initialize(string sectionName)
        {
            if (sectionLabel is not null)
            {
                sectionLabel.text = sectionName;
            }
        }
    }
}
