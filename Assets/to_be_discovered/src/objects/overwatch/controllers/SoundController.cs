using UnityEngine;

public class SoundController : MonoBehaviour {

    public static void makeSound(Sound sound, Vector2 sourcePos){
        handleTriggers(sound, sourcePos);
    }
    
    public static void makeSound(Sound sound, AudioSource source){
        handleTriggers(sound, source.gameObject.transform.position);
    }

    private static void handleTriggers(Sound sound, Vector2 sourcePos){
        if(!sound.isTrigger){
            return;
        }
        //do the triggering here
    }
    
}
