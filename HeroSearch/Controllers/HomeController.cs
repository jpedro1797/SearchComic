using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using searchcomic.MarvelApi;
using Microsoft.AspNetCore.Http;

namespace searchcomic.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Hero(string searchString)
        {

            if (string.IsNullOrWhiteSpace(searchString))
            {
                ModelState.AddModelError("", "Favor colocar um nome de herói.");
            }
            else
            {
                var _marvel = new MarvelApi.Marvel();
                CharacterDataWrapper CharacterModel = await _marvel.GetCharacters(searchString);
                Character _character = CharacterModel.Data.Results.FirstOrDefault();

                if (_character == null)
                {
                    ModelState.AddModelError("", "Herói não encontrado.");
                }
                else
                {
                    ComicDataWrapper comicModel = await _marvel.GetComicsForCharacter(_character.Id);
                    List<Comic> _comics = comicModel.Data.Results;
                    List<Comic> _ComicsWithImages = new List<Comic>(3);
                    int count = 0;
                    //some of the data is not in the api db. So skip the comics that do not have images and trim the description
                    foreach (var comic in _comics)
                    {
                        int pathlength = 0;
                        //if (count == 6)
                        //{
                        //    break;
                        //}

                        //Truncate the description if it is too long
                        if (!String.IsNullOrEmpty(comic.Description))
                        {
                            if (comic.Description.Length > 360)
                            {
                                comic.Description = comic.Description.Substring(0, 360) + " ...";
                            }
                        }

                        pathlength = comic.Thumbnail.Path.Length;

                        if (comic.Thumbnail.Path.Substring(pathlength - 19, 19) != "image_not_available")
                        {
                            _ComicsWithImages.Add(comic);
                            count++;
                        }
                    }

                    CharacterComicViewModel ccVM = new CharacterComicViewModel();

                    ccVM.Character = _character;
                    ccVM.Comics = _ComicsWithImages;
                    ccVM.AttributionText = CharacterModel.AttributionText;

                    HttpContext.Session.SetString("teste", "true");

                    ViewBag.Hero = true;

                    return View("Index", ccVM);
                }
            }
            return View("Index");
        }
    }
}
