using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ExplosionAnimation: MonoBehaviour
{
    public float frameRate = 0.07f;
    public Sprite[] frames;
    private SpriteRenderer renderer;
    private float nextFrameTime = 0;
    private int frameIndex = -1;
    private bool isRun = false;

    void Update()
    {
        if (nextFrameTime < Time.time && isRun)
        {
            nextFrameTime = Time.time + frameRate;
            frameIndex++;
            if (frameIndex == frames.Length)
            {
                isRun = false;
                renderer.sprite = null;
                frameIndex = -1;
                gameObject.SetActive(false);
                Destroy(gameObject); //create pool later
                return;
            }
            renderer.sprite = frames[frameIndex];
        }
    }

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    public void Play()
    {
        nextFrameTime = 0;
        isRun = true;
        //GetComponent<AudioSource>().PlayOneShot(sound);
    }
}

