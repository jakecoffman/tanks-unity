using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Tank : NetworkBehaviour {
    [SyncVar]
	public string playerName;
	[SyncVar]
    public Color color;

    public GameObject nameTagPrefab;
    public GameObject smokePrefab;

    const float speed = 20f;
    const float turnSpeed = 3.5f;

    GameObject _turret;
	GameObject _nameTag;
    bool _isFiring = false;
    Combat _combat;
    Rigidbody2D _rigid;

    void Awake()
    {
        _turret = transform.GetChild(0).gameObject;
        _combat = GetComponent<Combat>();
        _nameTag = Instantiate(nameTagPrefab, transform.position, Quaternion.identity) as GameObject;
    }

	void Start() {
        _rigid = GetComponent<Rigidbody2D>();
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
		{
			r.material.color = color;
		}
        if (isLocalPlayer)
        {
            _nameTag.GetComponentInChildren<TextMesh>().text = "You";
        }
        else
        {
            _nameTag.GetComponentInChildren<TextMesh>().text = playerName;
        }
        _nameTag.GetComponentInChildren<MeshRenderer>().enabled = true;
        _nameTag.GetComponentInChildren<Renderer>().sortingLayerName = "Player";
        ShowNameTags();
        StartCoroutine("FadeOut");
    }

    void ShowNameTags()
    {
        var renderer = _nameTag.GetComponentInChildren<Renderer>();
        var c = renderer.material.color;
        c.a = 1f;
        renderer.material.color = c;
    }

	IEnumerator FadeOut() {
		var renderer = _nameTag.GetComponentInChildren<Renderer> ();
		var opacity = 1f;
		while (opacity > 0) {
			yield return new WaitForSeconds (0.1f);
			opacity -= 0.05f;
			var c = renderer.material.color;
			c.a = opacity;
			renderer.material.color = c;
		}
	}

    void OnGUI()
    {
         _nameTag.transform.position = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            StopCoroutine("FadeOut");
            ShowNameTags();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            StartCoroutine("FadeOut");
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || _combat.isDead)
        {
            return;
        }
        Move();
        Aim();
        if (Input.GetMouseButton(0) && !_isFiring)
        {
            _isFiring = true;
            StartCoroutine(_combat.Fire(_turret.transform.position + _turret.transform.up * 0.6f, _turret.transform.rotation.eulerAngles));
            _isFiring = false;
        }
    }

    void Aim()
    {
        Vector3 mouse_pos = Input.mousePosition;
        mouse_pos.z = 0.0f;
        Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
        mouse_pos.x = mouse_pos.x - object_pos.x;
        mouse_pos.y = mouse_pos.y - object_pos.y;
        // -90 because my sprite is aiming up
        float angle = (Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg) - 90;
        Vector3 rotationVector = new Vector3(0, 0, angle);
        _turret.transform.rotation = Quaternion.Euler(rotationVector);
    }

    void Move()
    {
		if (_isFiring) {
			return;
		}

        float move = 0;
        if(Input.GetKey(KeyCode.W))
        {
            move = speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            move = -speed;
        }

        float rotation = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            rotation += -turnSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            rotation += turnSpeed;
        }
        if (speed > 0)
        {
            rotation *= -1;
        }

        _rigid.MoveRotation(_rigid.rotation + rotation);
        _rigid.AddForce(transform.up * move);
    }
}
