using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Social
{
    [CreateAssetMenu(fileName = "HashTag", menuName = "Wizards Code/Dev Logger/Create HashTag")]
    public class Hashtag : ScriptableObject
    {
        public string name;
        public bool selected = true;
    }
}
