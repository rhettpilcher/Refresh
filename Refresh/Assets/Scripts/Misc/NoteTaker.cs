using UnityEngine;

public class NoteTaker : MonoBehaviour
{
    [TextArea(3, 10)] // Adjust the number of rows (3) and maximum number of rows (10) as per your preference
    public string notes;
}