using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    private GameObject Camera1;      //�ォ�王�_�̃J�����i�[�p
    private GameObject Camera2;       //�L�����^�[���f���J�����i�[�p 

    // micro:bit�̃{�^���̏��(0: �Ȃ��A1: A�{�^���A-1: B�{�^��)
    private int buttonState = 0;

    //�Ăяo�����Ɏ��s�����֐�
    void Start()
    {
        //���ꂼ��̃J�������擾
        Camera1 = GameObject.Find("Camera1");
        Camera2 = GameObject.Find("Camera2");

        //�ォ�王�_�̃J�������A�N�e�B�u�ɂ���
        Camera1.SetActive(false);
    }


    //�P�ʎ��Ԃ��ƂɎ��s�����֐�
    void Update()
    {
        //�X�y�[�X�L�[�܂���B�{�^����������Ă���ԁA�ォ�王�_�̃J�������A�N�e�B�u�ɂ���
        if (Input.GetKey("space") || buttonState == -1)
        {
            //�ォ�王�_�̃J�������A�N�e�B�u�ɐݒ�
            Camera2.SetActive(false);
            Camera1.SetActive(true);
        }
        else
        {
            //�L�����^�[���f���J�������A�N�e�B�u�ɐݒ�
            Camera1.SetActive(false);
            Camera2.SetActive(true);
        }
    }

    //Bluetooth�ڑ��̐M�����擾
    public void OnButtonBChanged(int state)
    {
        buttonState = (state == 0 ? 0 : -1);
    }
}
