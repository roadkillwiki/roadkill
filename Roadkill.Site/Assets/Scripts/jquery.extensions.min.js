/*
 * jQuery Extensions 1.0
 * http://code.google.com/p/jquery-extensions/
 *
 * Copyright (c) 2009 C.Small
 *
 * Licensed under the MIT license.
 * Date: 23:21 15/09/2009
 * Minified with YUI minifier
 */
jQuery.isNumber=function(o){if(typeof o=="object"&&o!==null){return(typeof o.valueOf()==="number")}else{return(typeof o==="number")}};jQuery.isBoolean=function(o){if(typeof o=="object"&&o!==null){return(typeof o.valueOf()==="boolean")}else{return(typeof o==="boolean")}};jQuery.isNull=function(o){return(o===null)};jQuery.isUndefined=function(o){return(typeof o==="undefined")};jQuery.isNullOrUndefined=function(o){return jQuery.isNull(o)||jQuery.isUndefined(o)};jQuery.isString=function(o){return(typeof o==="string")};jQuery.isArray=function(o){return(o!=null&&typeof o=="object"&&"splice" in o&&"join" in o)};jQuery.emptyString=function(str){if(jQuery.isNullOrUndefined(str)){return true}else{if(!jQuery.isString(str)){throw"isEmpty: the object is not a string"}else{if(str.length===0){return true}}}return false};jQuery.startsWith=function(str,search){if(jQuery.isString(str)){return(str.indexOf(search)===0)}return false};jQuery.endsWith=function(str,search){if(!jQuery.isString(str)||!jQuery.isString(search)||jQuery.emptyString(str)||jQuery.emptyString(search)){return false}else{if(search.length>str.length){return false}else{if(str.length-search.length===str.lastIndexOf(search)){return true}}}return false};jQuery.formatString=function(){if(arguments.length<2){return""}var str=arguments[0];for(var i=1;i<arguments.length;i++){var val="";if(!jQuery.isNullOrUndefined(val)){val=arguments[i]+""}var regex=new RegExp("\\{"+(i-1)+"\\}","g");str=str.replace(regex,val)}return str};jQuery.log=function(){if(typeof console!=="undefined"){console.log(jQuery.formatString.apply(this,arguments))}};