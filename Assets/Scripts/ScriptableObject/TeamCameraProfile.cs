using UnityEngine;

[CreateAssetMenu(menuName = "Camera/Team Camera Profile")]
public class TeamCameraProfile : ScriptableObject
{
    public TeamName team;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
}
