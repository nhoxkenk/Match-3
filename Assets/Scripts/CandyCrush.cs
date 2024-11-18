using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyCrush
{
    public class CandyCrush: MonoBehaviour
    {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = false;

        [SerializeField] private Gem gemPrefab;
        [SerializeField] private GemType[] gemTypes;

        GridSystem2D<GridObject<Gem>> grid;

        InputReader inputReader;
        AudioManager audioManager;
        Vector2Int selectedGem = Vector2Int.one * -1;

        private void Awake()
        {
            inputReader = GetComponent<InputReader>();
            audioManager = GetComponent<AudioManager>();
        }

        void Start ()
        {
            InitializedGrid();
            inputReader.Fire += OnSelectGem;
        }

        private void OnDestroy()
        {
            inputReader.Fire -= OnSelectGem;
        }

        private void OnSelectGem()
        {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

            //validate position
            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos))
            {
                return;
            }

            if (selectedGem == gridPos)
            {
                DeselectGem();
                audioManager.PlayDeselect();
            }else if(selectedGem == Vector2Int.one * -1)
            {
                SelectGem(gridPos);
                audioManager.PlayClick();
            }
            else
            {
                StartCoroutine(RunGameLoop(selectedGem, gridPos));
            }
        }

        private bool IsEmptyPosition(Vector2Int gridPos) => grid.GetValue(gridPos.x, gridPos.y) == null;

        private bool IsValidPosition(Vector2Int gridPos) => gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < width && gridPos.y < height;

        private void DeselectGem() => selectedGem = new Vector2Int(-1, -1);

        private void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

        private IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            yield return StartCoroutine(SwapGem(gridPosA, gridPosB));

            //Swap animation
            //Matches ?
            List<Vector2Int> matches = FindMatches();
            //Make Gem Explode
            yield return StartCoroutine(ExplodeGame(matches));
            //Make gems fall
            yield return StartCoroutine(MakeGemsFall());
            //Replace empty spot
            yield return StartCoroutine(FillEmptySlot());

            //Is Game over ?
            DeselectGem();
        }

        private IEnumerator FillEmptySlot()
        {
            for(var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if(grid.GetValue(x, y) == null)
                    {
                        CreateGem(x, y);
                        audioManager.PlayPop();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        private IEnumerator MakeGemsFall()
        {
            for(var x = 0; x < width; x++)
            {
                for(var y = 0; y < height; y++)
                {
                    if(grid.GetValue(x, y) == null)
                    {
                        for(var i = y + 1; i < height; i++)
                        {
                            if(grid.GetValue(x, i) != null)
                            {
                                var gem = grid.GetValue(x, i).GetValue();
                                grid.SetValue(x, y, grid.GetValue(x, i));
                                grid.SetValue(x, i, null);
                                gem.transform.DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f)
                                    .SetEase(Ease.Linear);
                                yield return new WaitForSeconds(0.1f);
                                audioManager.PlayWoosh();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator ExplodeGame(List<Vector2Int> matches)
        {
            var duration = 0.25f;
            foreach(var match in matches)
            {
                var gem = grid.GetValue(match.x, match.y).GetValue();
                grid.SetValue(match.x, match.y, null);
                //ExplodeVFX(match);
                gem.transform.DOPunchScale(Vector3.one * 0.5f, duration, 1, 0.5f);
                audioManager.PlayPop();
                yield return new WaitForSeconds(duration);
                gem.DestroyGem();
            }
        }

        private List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new();

            //Horizontal
            for(var y = 0; y < height; y++)
            {
                for(var x = 0; x < width - 2; x++)
                {
                    var gemA = grid.GetValue(x, y);
                    var gemB = grid.GetValue(x + 1, y);
                    var gemC = grid.GetValue(x + 2, y);

                    if(gemA == null ||  gemB == null || gemC == null) { continue; }

                    if (gemA.GetValue().GetGemType() == gemB.GetValue().GetGemType()
                       && gemB.GetValue().GetGemType() == gemC.GetValue().GetGemType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x + 1, y));
                        matches.Add(new Vector2Int(x + 2, y));
                    }
                }
            }

            //Vertical
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height - 2; y++)
                {
                    var gemA = grid.GetValue(x, y);
                    var gemB = grid.GetValue(x, y + 1);
                    var gemC = grid.GetValue(x, y + 2);

                    if (gemA == null || gemB == null || gemC == null) { continue; }

                    if (gemA.GetValue().GetGemType() == gemB.GetValue().GetGemType()
                       && gemB.GetValue().GetGemType() == gemC.GetValue().GetGemType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x, y + 1));
                        matches.Add(new Vector2Int(x, y + 2));
                    }
                }
            }
            if(matches.Count > 0)
            {
                audioManager.PlayMatch();
            }
            else
            {
                audioManager.PlayNoMatch();
            }
            return new List<Vector2Int>(matches);
        }

        private IEnumerator SwapGem(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(Ease.InQuad);
            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(Ease.InQuad);

            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

            yield return new WaitForSeconds(0.5f);
        }

        private void InitializedGrid()
        {
            grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellSize, originPosition, debug);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    CreateGem(x, y);
                }
            }
        }

        private void CreateGem(int x, int y)
        {
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetGemType(gemTypes[UnityEngine.Random.Range(0, gemTypes.Length)]);

            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);

            grid.SetValue(x, y, gridObject);
        }
    }
}
