using System.Collections.Generic;
using UnityEngine;

public static class ConeCastExtension
{
    public static RaycastHit[] ConeCastAll(Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, maxDistance);
        List<RaycastHit> coneCastHitList = new List<RaycastHit>();

        if (sphereCastHits.Length > 0)
        {
            for (int i = 0; i < sphereCastHits.Length; i++)
            {
                sphereCastHits[i].collider.gameObject.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f);
                Vector3 hitPoint = sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);

                if (angleToHit < coneAngle)
                {
                    coneCastHitList.Add(sphereCastHits[i]);
                }
            }
        }

        RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
        coneCastHits = coneCastHitList.ToArray();

        return coneCastHits;
    }
    public static bool ConeCast(Vector3 origin, float maxRadius, Vector3 direction, out RaycastHit hit, float maxDistance, float coneAngle)
    {
        bool sphereCastHit = Physics.SphereCast(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, out hit, maxDistance);
        if (sphereCastHit == false)
            return false;

        Vector3 hitPoint = hit.point;
        Vector3 directionToHit = hitPoint - origin;
        float angleToHit = Vector3.Angle(direction, directionToHit);

        return angleToHit <= coneAngle;
    }
}