using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Camera))]
[System.Serializable]
public class PictureCamera : MonoBehaviour
{
    Camera pictureCam;

    int resWidth = 256;
    int resHeight = 256;
    int depth = 24;
    
    private void Awake()
    {
        pictureCam = GetComponent<Camera>();

        if (pictureCam.targetTexture == null)
        {
            pictureCam.targetTexture = new RenderTexture(resWidth, resHeight, depth);
        }
        else
        {
            resWidth = pictureCam.targetTexture.width;
            resHeight = pictureCam.targetTexture.height;
        }

        pictureCam.gameObject.SetActive(false);
    }

    public void TakePicture()
    {
        pictureCam.gameObject.SetActive(true);
    }

    public void LateUpdate()
    {
        if (pictureCam.gameObject.activeInHierarchy)
        {
            string dateTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string folderName = "/Photos/";
            string fileName = Application.dataPath + folderName + "Photo_" + dateTime + ".png";

            Texture2D picture = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false); //Make a placeholder blank photo
            pictureCam.Render(); //The camera wasn't on this frame so we need to manually render
            RenderTexture.active = pictureCam.targetTexture; //Set the active render texture 
            picture.ReadPixels(new Rect(0,0, resWidth, resHeight), 0, 0); //Will fill in pixels from bottom left (0,0) to top right (width, height)

            //Actually throw it all together into a file
            byte[] bytes = picture.EncodeToPNG();
            System.IO.File.WriteAllBytes(fileName, bytes);

            pictureCam.gameObject.SetActive(false);
        }
    }
}
