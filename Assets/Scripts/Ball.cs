using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using UnityEngine.EventSystems;
//using Es.InkPainter.Sample;
using DG.Tweening;

namespace UnityStandardAssets.Vehicles.Ball
{
    public class Ball : MonoBehaviour
    {
        Rigidbody rigid;
        public GameObject explodeEffect;
        public Color ballColor;
        float force;
        public Vector3 maxVelocity = new Vector3(0,50,0);
        bool isHit = false;
        private void OnEnable()
        {
            //GetComponent<CollisionPainter>().brush.Color = ballColor;
            transform.GetChild(0).GetComponent<TrailRenderer>().startColor = ballColor;
            transform.GetChild(0).GetComponent<TrailRenderer>().endColor = new Color32((byte)ballColor.r, (byte)ballColor.g, (byte)ballColor.b, 0);
            rigid = GetComponent<Rigidbody>();
            force = 1000;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Stack")
            {
                if (!isHit)
                {
                    MMVibrationManager.Vibrate();
                    SoundManager.instance.PlaySound(SoundManager.instance.ball);
                    var explode = Instantiate(explodeEffect, transform.position, Quaternion.Euler(0, 90, -90));
                    var explode1 = explode.transform.GetChild(0);
                    var explode2 = explode.transform.GetChild(1);
                    var mod1 = explode.GetComponent<ParticleSystem>().main;
                    mod1.startColor = ballColor * 1.5f;
                    var mod2 = explode1.GetComponent<ParticleSystem>().main;
                    mod2.startColor = ballColor * 1.5f;
                    var mod3 = explode2.GetComponent<ParticleSystem>().main;
                    mod3.startColor = ballColor * 1.5f;
                    rigid.velocity = new Vector3(0, 55, 0);
                    //Debug.Log(rigid.velocity);
                    transform.DOKill();
                    transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    transform.DOPunchScale(new Vector3(0, 1f, 0), 0.5f)/*.SetLoops(2,LoopType.Yoyo)*/;
                    transform.GetChild(0).GetComponent<TrailRenderer>().enabled = true;
                    isHit = true;
                }
            }

            else if (other.gameObject.tag == "DeadStack")
            {
                if (!isHit)
                {
                    MMVibrationManager.Vibrate();
                    SoundManager.instance.PlaySound(SoundManager.instance.ball);
                    var explode = Instantiate(explodeEffect, transform.position, Quaternion.Euler(0, 90, -90));
                    var explode1 = explode.transform.GetChild(0);
                    var explode2 = explode.transform.GetChild(1);
                    var mod1 = explode.GetComponent<ParticleSystem>().main;
                    mod1.startColor = ballColor * 1.5f;
                    var mod2 = explode1.GetComponent<ParticleSystem>().main;
                    mod2.startColor = ballColor * 1.5f;
                    var mod3 = explode2.GetComponent<ParticleSystem>().main;
                    mod3.startColor = ballColor * 1.5f;
                    Spawner.instance.RemoveBall(gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "DeadStack")
            {
                MMVibrationManager.Vibrate();
                SoundManager.instance.PlaySound(SoundManager.instance.ball);
                var explode = Instantiate(explodeEffect, transform.position, Quaternion.Euler(0, 90, -90));
                var explode1 = explode.transform.GetChild(0);
                var explode2 = explode.transform.GetChild(1);
                var mod1 = explode.GetComponent<ParticleSystem>().main;
                mod1.startColor = ballColor * 1.5f;
                var mod2 = explode1.GetComponent<ParticleSystem>().main;
                mod2.startColor = ballColor * 1.5f;
                var mod3 = explode2.GetComponent<ParticleSystem>().main;
                mod3.startColor = ballColor * 1.5f;
                //Spawner.instance.RemoveBall(gameObject);
                transform.position = new Vector3(transform.position.x, other.transform.position.y + 1, transform.position.z);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            isHit = false;
        }
    }
}
