using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data.Components.Textures;
using UnityEngine;

public class CharacterAnimationData : AnimationData
{
    /// <summary>
    /// The frames per second of walking
    /// </summary>
    public float walkFps = 1.0f / 3.0f;

    /// <summary>
    /// The attack frames per second
    /// </summary>
    public float attackFps = 1.0f / 4.0f;


    public Sprite[] maskFrames;
    public CharacterAnimationData(SpriteWorldObject worldObject, CharacterAnimation animation) : base(worldObject, animation)
    {
        UpdateFrames();
    }

    public override void SetState(AnimationState newState, AnimationDirection newDirection, float attackCd)
    {
        if (state == newState)
        {
            
            SetDirection(newDirection, attackCd);
            return;
        }
        state = newState;
        direction = newDirection;

        if (state == AnimationState.Attack)
        {
            GetAttackData(attackCd, out frame, out remainingFrameTime);
        }
        else
        {
            ResetTime(attackCd);
        }
        
        UpdateFrames();
        if (((CharacterAnimation)animation).hasMask)
        {
            UpdateMaskFrames();
        }
    }



    public override void Update(float deltaTime)
    {
        remainingFrameTime -= deltaTime;
        while (remainingFrameTime < 0)
        {
            frame++;
            if (frame >= frames.Length)
                Loop();
            remainingFrameTime += GetFps();
            UpdateSprite();
            if (((CharacterAnimation)animation).hasMask)
            {
                UpdateMaskSprite();
            }
        }
    }


    public override void ResetTime(float attackCd)
    {
        if (state == AnimationState.Attack)
            GetAttackData(attackCd, out frame, out remainingFrameTime);
        else
            base.ResetTime(attackCd);
    }

    protected override void SetDirection(AnimationDirection newDirection, float attackCd)
    {
        if (state == AnimationState.Attack)
        {
            int oldFrame = frame;
            GetAttackData(attackCd, out frame, out remainingFrameTime);
            if (oldFrame != frame && direction == newDirection)
            {
                UpdateFrames();
                if (((CharacterAnimation)animation).hasMask)
                {
                    UpdateMaskFrames();
                }
            }
            else
            {
                if (direction == newDirection) return;
                direction = newDirection;

                ResetTime(attackCd);

                UpdateFrames();
                if (((CharacterAnimation)animation).hasMask)
                {
                    UpdateMaskFrames();
                }
            }
        }
        else {
            if (direction == newDirection) return;
            direction = newDirection;

            ResetTime(attackCd);

            UpdateFrames();
            if (((CharacterAnimation)animation).hasMask)
            {
                UpdateMaskFrames();
            }
        }
    }



    protected override float GetFps()
    {
        return state == AnimationState.Attack ? attackFps : walkFps;
    }

    private void GetAttackData(float attackCd, out int frame, out float remainingFrameTime)
    {
        if (attackCd > attackFps)
        {
            frame = 0;
            remainingFrameTime = attackCd - attackFps;
        }
        else
        {
            frame = 1;
            remainingFrameTime = attackCd;
        }
    }

    protected void UpdateMaskFrames()
    {
        maskFrames = ((CharacterAnimation)animation).GetMaskFrames(state, direction);
        UpdateMaskSprite();
    }
    protected void UpdateMaskSprite()
    {
        worldObject.SetMask(maskFrames[Math.Max(Math.Min(frame, maskFrames.Length - 1), 0)], !noFlip && direction == AnimationDirection.Left);
    }
}

public class CharacterAnimation : Animation
{
    /// <summary>
    /// Still animations
    /// </summary>
    public readonly Sprite[] stillSide;
    public readonly Sprite[] stillUp;
    public readonly Sprite[] stillDown;

    /// <summary>
    /// Walk animations
    /// </summary>
    public readonly Sprite[] walkSide;
    public readonly Sprite[] walkUp;
    public readonly Sprite[] walkDown;

    /// <summary>
    /// Attack animations
    /// </summary>
    public readonly Sprite[] attackSide;
    public readonly Sprite[] attackUp;
    public readonly Sprite[] attackDown;

    /// <summary>
    /// All states
    /// </summary>
    public readonly Sprite[] allSide;
    public readonly Sprite[] allUp;
    public readonly Sprite[] allDown;

        /// <summary>
    /// If the texture has a sprite mask, these textures match
    /// the ones of the sprite but must be stored outside
    /// of the sprite atlises because they compression will screw up shaders
    /// </summary>
    public bool hasMask = false;
    public readonly Sprite[] mask_stillSide;
    public readonly Sprite[] mask_stillUp;
    public readonly Sprite[] mask_stillDown;

    /// <summary>
    /// Walk animations
    /// </summary>
    public readonly Sprite[] mask_walkSide;
    public readonly Sprite[] mask_walkUp;
    public readonly Sprite[] mask_walkDown;

    /// <summary>
    /// Attack animations
    /// </summary>
    public readonly Sprite[] mask_attackSide;
    public readonly Sprite[] mask_attackUp;
    public readonly Sprite[] mask_attackDown;

    /// <summary>
    /// All states
    /// </summary>
    public readonly Sprite[] mask_allSide;
    public readonly Sprite[] mask_allUp;
    public readonly Sprite[] mask_allDown;

    public readonly Sprite[] mask_frames;

    public CharacterAnimation(CharacterTextureData textureData)
    {
        
        stillSide = new Sprite[] { TextureManager.GetSprite(textureData.spriteSetName + "-1") };
        stillUp = new Sprite[] { TextureManager.GetSprite(textureData.spriteSetName + "-5") };
        stillDown = new Sprite[] { TextureManager.GetSprite(textureData.spriteSetName + "-10") };

        walkSide = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-2"),
                stillSide[0]
        };
        walkUp = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-6"),
                TextureManager.GetSprite(textureData.spriteSetName + "-7")
        };
        walkDown = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-11"),
                TextureManager.GetSprite(textureData.spriteSetName + "-12")
        };

        attackSide = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-3"),
                TextureManager.GetSprite(textureData.spriteSetName + "-4")
        };
        attackUp = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-8"),
                TextureManager.GetSprite(textureData.spriteSetName + "-9")
        };
        attackDown = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-13"),
                TextureManager.GetSprite(textureData.spriteSetName + "-14")
        };

        allSide = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-1"),
                TextureManager.GetSprite(textureData.spriteSetName + "-2"),
                TextureManager.GetSprite(textureData.spriteSetName + "-4"),
                TextureManager.GetSprite(textureData.spriteSetName + "-3")
        };
        allUp = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-5"),
                TextureManager.GetSprite(textureData.spriteSetName + "-6"),
                TextureManager.GetSprite(textureData.spriteSetName + "-7"),
                TextureManager.GetSprite(textureData.spriteSetName + "-9"),
                TextureManager.GetSprite(textureData.spriteSetName + "-8")
        };
        allDown = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-10"),
                TextureManager.GetSprite(textureData.spriteSetName + "-11"),
                TextureManager.GetSprite(textureData.spriteSetName + "-12"),
                TextureManager.GetSprite(textureData.spriteSetName + "-14"),
                TextureManager.GetSprite(textureData.spriteSetName + "-13")
        };

        if (textureData.hasMask)
        {
            Debug.Log("mask??? " + textureData.hasMask);
            Debug.Log("texture name: " + textureData.spriteSetName);
        }
        
        
        if (textureData.hasMask)
        {
            hasMask = true;
            string texturemaskname = "SpriteMasks/" + textureData.spriteSetName + "Mask/" + textureData.spriteSetName + "Mask";
            //string texturename = "/SpriteMasks/"textureData.spriteSetName;
            mask_stillSide = new Sprite[] { Resources.Load<Sprite>(texturemaskname+ "-1") };
            mask_stillUp = new Sprite[] { Resources.Load<Sprite>(texturemaskname + "-5") };
            mask_stillDown = new Sprite[] { Resources.Load<Sprite>(texturemaskname + "-10") };
            Debug.Log(texturemaskname);
            mask_walkSide = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-2"),
                mask_stillSide[0]
            };
            mask_walkUp = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-6") ,
                Resources.Load<Sprite>(texturemaskname+ "-7") 
            };
            mask_walkDown = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-11") ,
                Resources.Load<Sprite>(texturemaskname+ "-12")
            };

            mask_attackSide = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-3") ,
                Resources.Load<Sprite>(texturemaskname+ "-4") 
            };
            mask_attackUp = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-8") ,
                Resources.Load<Sprite>(texturemaskname+ "-9") 
            };
            mask_attackDown = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-12") ,
                Resources.Load<Sprite>(texturemaskname+ "-14")
            };

            mask_allSide = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-1") ,
                Resources.Load<Sprite>(texturemaskname+ "-2") ,
                Resources.Load<Sprite>(texturemaskname+ "-4") ,
                Resources.Load<Sprite>(texturemaskname+ "-3")
            };
            mask_allUp = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-5") ,
                Resources.Load<Sprite>(texturemaskname+ "-6") ,
                Resources.Load<Sprite>(texturemaskname+ "-7"),
                Resources.Load<Sprite>(texturemaskname+ "-9"),
                Resources.Load<Sprite>(texturemaskname+ "-8")
            };
            mask_allDown = new Sprite[]
            {
                Resources.Load<Sprite>(texturemaskname+ "-10"),
                Resources.Load<Sprite>(texturemaskname+ "-11"),
                Resources.Load<Sprite>(texturemaskname+ "-12"),
                Resources.Load<Sprite>(texturemaskname+ "-14"),
                Resources.Load<Sprite>(texturemaskname+ "-13")
            };
        }

    }



    public override Sprite[] GetFrames(AnimationState state, AnimationDirection direction)
    {
        switch (state)
        {
            case AnimationState.Walk:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return walkUp;
                    case AnimationDirection.Down:
                        return walkDown;
                    default:
                        return walkSide; 
                }
            case AnimationState.Attack:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return attackUp;
                    case AnimationDirection.Down:
                        return attackDown;
                    default:
                        return attackSide;
                }
            case AnimationState.All:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return allUp;
                    case AnimationDirection.Down:
                        return allDown;
                    default:
                        return allSide;
                }
            default:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return stillUp;
                    case AnimationDirection.Down:
                        return stillDown;
                    default:
                        return stillSide;
                }
        }
    }

    public Sprite[] GetMaskFrames(AnimationState state, AnimationDirection direction)
    {
        switch (state)
        {
            case AnimationState.Walk:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return mask_walkUp;
                    case AnimationDirection.Down:
                        return mask_walkDown;
                    default:
                        return mask_walkSide;
                }
            case AnimationState.Attack:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return mask_attackUp;
                    case AnimationDirection.Down:
                        return mask_attackDown;
                    default:
                        return mask_attackSide;
                }
            case AnimationState.All:
                switch (direction) 
                {
                    case AnimationDirection.Up:
                        return mask_allUp;
                    case AnimationDirection.Down:
                        return mask_allDown;
                    default:
                        return mask_allSide;
                }
            default:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return mask_stillUp;
                    case AnimationDirection.Down:
                        return mask_stillDown;
                    default:
                        return mask_stillSide;
                }
        }
    }
}