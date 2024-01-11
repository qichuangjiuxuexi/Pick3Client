using UnityEngine;
using UnityEngine.UI;

public class MyImage : MonoBehaviour
{
    private Material imagematerial;

    public Material Imagematerial
    {
        get { return imagematerial; }
        set
        {
            imagematerial = value;
            this.GetComponent<Image>().material = imagematerial;
        }
    }

    private float imagealpha;

    public float Imagealpha
    {
        get { return imagealpha; }
        set
        {
            imagealpha = value;
            this.GetComponent<Image>().material.SetFloat("_Alpha", imagealpha);
        }
    }
}