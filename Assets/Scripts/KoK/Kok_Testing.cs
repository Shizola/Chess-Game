using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kok_Testing : MonoBehaviour
{
    public int centipawnScore;

    // Start is called before the first frame update
    void Update()
    {
            if (centipawnScore <= -1000) 
            {        
                Debug.Log("Extremely Worse Position");
            }
            else if (centipawnScore <= -500 && centipawnScore > -1000) 
            {
                Debug.Log("Much Worse Position");
            }
            else if (centipawnScore <= -150 && centipawnScore  > -500)
            {           
                Debug.Log("Worse Position");
            }
            else if (centipawnScore <=-50 && centipawnScore > -150)
            {           
                Debug.Log("Slightly Worse Position");
            }
            else if (centipawnScore < 50 && centipawnScore > -50) 
            {           
                Debug.Log("Neutral Position");
            }
            else if (centipawnScore >= 50 && centipawnScore < 150) 
            {         
                Debug.Log("Slightly Better Position");
            }
            else if (centipawnScore >= 150 && centipawnScore < 500) 
            {       
                Debug.Log("Better Position");
            }            
            else if (centipawnScore >= 500 && centipawnScore < 1000) 
            {
                Debug.Log("Much Better Position");
            }
            else if(centipawnScore >= 1000)
            {       
                Debug.Log("Extremely Better Position");
            }        
    }
}
