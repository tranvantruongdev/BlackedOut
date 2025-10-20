using UnityEngine;

public class Player : MonoBehaviour
{
    // Reference to PlayerSkinApplier
    [Header("Skin")]
    public PlayerSkinApplier skinApplier;

    void Start()
    {
        // Đảm bảo skin được áp dụng khi player được tạo
        if (skinApplier != null)
        {
            skinApplier.ApplyEquippedNow();
        }
    }

    //trigger game over if ball hit by smasher
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && collision.gameObject.transform.position.y > transform.position.y)
        {
            GameManager.S_Instance.GameOverAction();
        }
    }
}
