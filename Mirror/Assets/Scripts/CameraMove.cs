using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float upDownSpeed = 3f;
    public float mouseSensitivity = 100f;

    private float _yRotation;

    private void Awake()
    {
        // 锁定鼠标并隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化水平旋转角度
        _yRotation = transform.localEulerAngles.y;
    }

    private void Update()
    {
        // ===== 1. 鼠标左右旋转视角（关闭上下俯仰） =====
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        _yRotation += mouseX; // 仅保留水平旋转，X轴角度固定为0，禁用上下转头
        transform.localRotation = Quaternion.Euler(0f, _yRotation, 0f);

        // ===== 2. WASD 前后左右移动 =====
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var moveDir = transform.right * h + transform.forward * v;
        moveDir.y = 0;
        moveDir.Normalize(); // 防止斜向移动速度叠加变快
        transform.Translate(moveDir * (moveSpeed * Time.deltaTime), Space.World);

        // ===== 3. QE 垂直升降 =====
        float upDown = 0;
        if (Input.GetKey(KeyCode.Q)) upDown = -1;
        if (Input.GetKey(KeyCode.E)) upDown = 1;
        transform.Translate(Vector3.up * (upDown * upDownSpeed * Time.deltaTime), Space.World);

        // 按下 ESC 解锁鼠标光标，方便编辑操作
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}