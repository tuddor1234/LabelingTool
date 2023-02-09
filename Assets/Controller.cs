using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public Button LeftButton;
    public Button RightButton;
    public Button BrowseButton;
    public Button SaveButton;
    public Image Img;

    public GameObject RectangePrefab;

    Canvas canvas;
    GameObject CurrentBox;
    Vector3 StartLocation;
    Vector2 StartAnchoredLocation;

    string workingDirectory = null;

    [System.Serializable]
    public class CustomRectangle
    {
        public int X1, Y1, X2, Y2;
    }

    FileInfo[] Paths;
    int pathIndex = 0;

    List<GameObject> Boxes = new List<GameObject>();
    private bool bCanBeResized;
    private float scaleFactor;

    private void Awake()
    {
        LeftButton.onClick.AddListener(Left);
        RightButton.onClick.AddListener(Right);
        BrowseButton.onClick.AddListener(Browse);
        SaveButton.onClick.AddListener(SaveBoxesToDisk);

        canvas = GetComponent<Canvas>();
    }

    private void Browse()
    {
        string currentPath = Application.dataPath + "/../Resources";
        workingDirectory = EditorUtility.OpenFolderPanel("Browse the folder where you have the images", currentPath, "Images");
        var info = new DirectoryInfo(workingDirectory);
        Paths = info.GetFiles();
        pathIndex = 0;
        print(Paths.Length);
        ReloadImage();
    }

    void Left()
    {
        SaveBoxesToDisk();

        if (Paths == null)
            return;

        pathIndex--;
        if (pathIndex < 0)
        {
            pathIndex = Paths.Length - 1;
        }
        ReloadImage();
    }

    void Right()
    {
        SaveBoxesToDisk();

        if (Paths == null)
            return;

        pathIndex++;
        pathIndex = pathIndex % Paths.Length;
        ReloadImage();
    }

    private void ReloadImage()
    {
   
        Texture2D Tex2D;
        byte[] FileData;
        if (File.Exists(Paths[pathIndex].FullName))
        {
            FileData = File.ReadAllBytes(Paths[pathIndex].FullName);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
            {
                Img.sprite = Sprite.Create(Tex2D, new Rect(0, 0, Tex2D.width, Tex2D.height), Vector2.zero);
            }
            var imgHeight = Img.GetComponent<RectTransform>().rect.height;
            scaleFactor = Tex2D.height / imgHeight;
        }
        foreach(var box in Boxes)
        {
            Destroy(box);
        }
        Boxes.Clear();
    }



    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.y < 200)
                return;

            DrawNewRectangle();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            bCanBeResized = false;
        }

        if (bCanBeResized)
        {
            
            Vector2 diff = StartLocation - Input.mousePosition;
            // CurrentBox.GetComponent<RectTransform>().
            var rt = CurrentBox.GetComponent<RectTransform>();

     
            Vector2 NewSize = new Vector2(Math.Abs(diff.x), Math.Abs(diff.y));
        
            Vector2 SizeDiff = new Vector2(rt.rect.width - NewSize.x, rt.rect.height - NewSize.y);
            rt.sizeDelta = NewSize;
            rt.anchoredPosition +=  new Vector2(-SizeDiff.x , SizeDiff.y) / 2;

         
        }

      //  print(Input.mousePosition);
   

    }
   
       
    private void SaveBoxesToDisk()
    {
        string json = "[";
        //List<CustomRectangle> AllBoxes = new List<CustomRectangle>();
        for (int i = 0; i < Boxes.Count; i++ )
        {
            var rt = Boxes[i].GetComponent<RectTransform>();
            int xtop = (int)((rt.anchoredPosition.x + rt.rect.width / 2) * scaleFactor) ;
            int ytop = (int)((rt.anchoredPosition.y + rt.rect.height / 2) * scaleFactor);

            int xbot = (int)((rt.anchoredPosition.x - rt.rect.width / 2) * scaleFactor);
            int ybot = (int)((rt.anchoredPosition.y - rt.rect.height / 2) * scaleFactor);

            CustomRectangle rect = new CustomRectangle() { X1 = xtop, Y1 = ytop, X2 = xbot, Y2 = ybot };
           
            json += JsonUtility.ToJson(rect, true); 
            if(i != Boxes.Count -1)
            {    
                json += ",\n";
            }
        }
        json += "]";
  
        if (workingDirectory == null)
        {
            workingDirectory = Directory.GetCurrentDirectory() + "/Progress/";
            if (!Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }
        }

        string ext = "." + Paths[pathIndex].Extension;
        string name = Paths[pathIndex].Name.Trim(ext.ToCharArray());
        File.WriteAllText(workingDirectory + "/" + name+".json", json);




    }


    private void DrawNewRectangle()
    {
        StartLocation = Input.mousePosition; StartLocation.z = 0;
        CurrentBox = Instantiate(RectangePrefab, StartLocation, Quaternion.identity);
        CurrentBox.transform.SetParent(Img.transform);
        StartAnchoredLocation = CurrentBox.GetComponent<RectTransform>().anchoredPosition;

        CurrentBox.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        Boxes.Add(CurrentBox);
        bCanBeResized = true;
    }

 
}
