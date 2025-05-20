using System.Collections;
using UnityEngine;

public class MyCoroutineManager : MonoBehaviour
{
    public void StartPieceCooldown(Piece piece, float duration)
    {
        StartCoroutine(PieceCooldownCoroutine(piece, duration));
    }

    private IEnumerator PieceCooldownCoroutine(Piece piece, float duration)
    {
        piece.BeginAttackCooldown();
        yield return new WaitForSeconds(duration);
        piece.EndAttackCooldown();
    }
}

